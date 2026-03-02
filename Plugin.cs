using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Handlers;
using UpgradesLIB;
using UpgradesLIB.Items.Equipment;

namespace LaserCutterUpgrades;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
[BepInDependency("com.lawabidingmodder.upgradeslib")]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger { get; private set; }

    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    
    public static readonly GameInput.Button OpenUpgradesButton = EnumHandler.AddEntry<GameInput.Button>("OpenLCUpgrades")
        .CreateInput("Open LaserCutter Upgrades")
        .WithKeyboardBinding(GameInputHandler.Paths.Keyboard.B)
        .WithCategory("Tools Upgrades");

    public static readonly TechCategory LaserCutterUpgrades = EnumHandler.AddEntry<TechCategory>("LaserCutterUpgrades").WithPdaInfo("Laser Cutter Upgrades").RegisterToTechGroup(UpgradesLIB.Plugin.toolupgrademodules);
        
    public static EquipmentType EquipmentType = EquipmentType.None;
    public const string StorageName = "LaserCutterContainer";
    public const string StorageClassID = "LaserCutterContainerClassID";
    public static readonly Dictionary<TechType, float> Multipliers = new();

    private void Awake()
    {
        Logger = base.Logger;

        StartCoroutine(Utilities.CreateUpgradesContainer<LaserCutterUpgradeInput>(
            TechType.LaserCutter, "LaserCutterUpgrade", StorageName, StorageClassID, 
            "LASER CUTTER", 2, this));
        
        CraftTreeHandler.AddTabNode(Handheldprefab.HandheldfabTreeType, 
            "LaserCutterTab", "Laser Cutter", SpriteManager.Get(TechType.LaserCutter), 
            "Tools");
        
        Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
        
        InitializePrefabs();
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is loaded!");
    }

    public void InitializePrefabs()
    {
        EquipmentType = Utilities.ClaimEquipmentTypes(this)[0];
        var currentMultiplier = 2f;
        List<Ingredient>[] ingredients = { new(){new(TechType.Battery, 1), 
                new(TechType.WiringKit, 1)}, 
            new(){new (TechType.Lubricant, 1), new(TechType.WiringKit, 1)},
            new(){new(TechType.Aerogel, 1), new(TechType.AdvancedWiringKit, 1)},
            new(){new(TechType.Silicone,1), new(TechType.WiringKit,1)}
        };
        TechType prevSpeed = 0;
        TechType prevEnergy = 0;
        
        for (int i = 0; i < 3; i++)
        {
            var info = PrefabInfo.WithTechType($"LaserSpeedUpgradeMk{i+1}", 
                    $"Laser Cutter Speed Upgrade Mk {i+1}", $"Mk {i+1}"
                    + $" speed upgrade for the Laser Cutter. Decreases the cutting speed by " +
                    $"{currentMultiplier}x")
                .WithIcon(SpriteManager.Get(TechType.LaserCutter));
            Multipliers.Add(info.TechType, 100+currentMultiplier);
            
            var speedIngredient = i > 0 ? new Ingredient(prevSpeed, 1) : null;
            if (i > 0) ingredients[i].Add(speedIngredient);
            new LaserCutterUpgrade(info).Register(ingredients[i]);
            
            prevSpeed = info.TechType;
            info = PrefabInfo.WithTechType($"LaserEnergyUpgradeMk{i+1}",
                $"Laser Cutter Energy Upgrade Mk {i+1}", 
                $"Mk {i+1} energy upgrade for the Laser Cutter. Decreases the energy " +
                $"usage by {currentMultiplier}x")
                .WithIcon(SpriteManager.Get(TechType.LaserCutter));
            Multipliers.Add(info.TechType, 200+currentMultiplier);
            
            var energyIngredients = i == 1 ? ingredients[i+2] : ingredients[i];
            if (i > 0)
            {
                energyIngredients.Remove(speedIngredient);
                energyIngredients.Add(new Ingredient(prevEnergy, 1));
            }
            new LaserCutterUpgrade(info).Register(energyIngredients);
            
            prevEnergy = info.TechType;
            currentMultiplier += i + 1;
        }

        var drillableInfo = PrefabInfo.WithTechType("LaserCutterDrillUpgrade",
            "Laser Cutter Drilling Upgrade",
            "Drilling upgrade for the laser cutter. Unlocks the ability to mine " +
            "drillables with the laser cutter");
        Multipliers.Add(drillableInfo.TechType, 6767);
        new LaserCutterUpgrade(drillableInfo.WithIcon(SpriteManager.Get(TechType.ExosuitDrillArmModule)))
            .Register(new List<Ingredient>()
            {
                new(TechType.Diamond, 5), new(TechType.Lubricant, 3), 
                new(TechType.Aerogel, 2), new(TechType.AdvancedWiringKit, 2),
                new(TechType.AramidFibers, 2), new(TechType.PlasteelIngot, 1)
            });
    }
}