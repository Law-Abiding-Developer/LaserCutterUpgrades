using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UpgradesLIB;

namespace LaserCutterUpgrades;

[HarmonyPatch(typeof(LaserCutter))]
public class LaserCutterPatches
{
    private static Dictionary<LaserCutter,float[]> _timers = new();
    [HarmonyPatch(nameof(LaserCutter.OnToolUseAnim)), HarmonyPrefix]
    public static bool OnToolUseAnimPrefix(LaserCutter __instance)
    {
        return false;
    }

    [HarmonyPatch(nameof(LaserCutter.Update)), HarmonyPostfix]
    public static void UpdatePostfix(LaserCutter __instance)
    {
        if (__instance == null) return;
        if (!_timers.ContainsKey(__instance)) _timers.Add(__instance, new[]{0f,0});
        if (__instance.usedThisFrame) _timers[__instance][0] += Time.deltaTime;
        
        var panel = Utilities.GetPanel<LaserCutterUpgradeInput>(__instance.gameObject, 
            Plugin.StorageName, Plugin.StorageClassID);
        if (panel == null) return;
        if (__instance.usedThisFrame && panel.EnableDrillPatch()) 
            _timers[__instance][1] += Time.deltaTime;
        var timeToWeld = 0.18f / panel.GetHighestSpeedMultiplier();
        
        if (__instance.usedThisFrame && _timers[__instance][0] >= timeToWeld)
        {
            __instance.LaserCut();
            _timers[__instance][0] = 0;
        }

        if (panel.EnableDrillPatch())
        {
            GameObject gameObject = null;
            Vector3 vector = Vector3.zero;
            UWE.Utils.TraceFPSTargetPosition(Player.main.gameObject, 3.5f, 
                ref gameObject, ref vector);
            if (gameObject)
            {
                var drillable = gameObject.GetComponentInParent<Drillable>();
                if (drillable)
                {
                    if (_timers[__instance][1] >= 0.1f)
                    {
                        _timers[__instance][1] = 0f;
                        var entityRoot = UWE.Utils.GetEntityRoot(gameObject) ?? gameObject;
                        entityRoot?.GetComponentInChildren<Drillable>()?.OnDrill(vector, null, out var _);
                    }

                    __instance.energyMixin.ConsumeEnergy(Time.deltaTime);
                }
            }
        }
        
        if (panel.equipment == null) return;
        if (GameInput.GetButtonDown(Plugin.OpenUpgradesButton)) panel.OpenPDA();
    }

    [HarmonyPatch(nameof(LaserCutter.LaserCut)), HarmonyPrefix]
    public static void LaserCutPrefix(LaserCutter __instance)
    {
        var panel = Utilities.GetPanel<LaserCutterUpgradeInput>(__instance.gameObject, 
            Plugin.StorageName, Plugin.StorageClassID);
        if (panel == null) return;
        __instance.laserEnergyCost = 0.18f * Mathf.Sqrt(panel.GetHighestSpeedMultiplier())
                                     /panel.GetHighestEnergyMultiplier();
        __instance.healthPerWeld = 1f;
    }
}

[HarmonyPatch(typeof(Drillable))]
public class DrillablePatches
{
    [HarmonyPatch(nameof(Drillable.HoverDrillable)), HarmonyPostfix]
    public static void HoverDrillablePostfix(Drillable __instance)
    {
        if (Inventory.main.GetHeldTool() is not LaserCutter cutter) return;
        if (!Utilities.GetPanel<LaserCutterUpgradeInput>(cutter.gameObject,
                Plugin.StorageName, Plugin.StorageClassID).EnableDrillPatch()) return;
        GameInput.Button button = GameInput.Button.RightHand;
        HandReticle.main.SetText(HandReticle.TextType.Hand, 
            Language.main.GetFormat("DrillResource", Language.main.Get(__instance.primaryTooltip)), 
            false, button);
        HandReticle.main.SetText(HandReticle.TextType.HandSubscript,
            __instance.secondaryTooltip, true);
        HandReticle.main.SetIcon(HandReticle.IconType.Drill);

    }
}