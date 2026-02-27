using System;
using System.Collections.Generic;
using UpgradesLIB;

namespace LaserCutterUpgrades;

public class LaserCutterUpgradeInput : ModdedUpgradeConsoleInput
{
    private List<float> _highestSpeedMultiplier = new();
    private List<float> _highestEnergyMultiplier = new();
    private bool _enableDrillPatch = false;
    
    public override void InitializeEquipment()
    {
        _highestSpeedMultiplier.Add(1);
        _highestEnergyMultiplier.Add(1);
        base.InitializeEquipment();
        equipment.onAddItem += OnAddItem;
        equipment.onRemoveItem += OnRemoveItem;
    }

    public void OnAddItem(InventoryItem item)
    {
        if (!Plugin.Multipliers.TryGetValue(item.techType, out float value)) return;
        
        if (value > 100 && value < 200)
        {
            if (value > _highestSpeedMultiplier[0]) _highestSpeedMultiplier[0] = value-100;
            else _highestSpeedMultiplier.Add(value-100);
            _highestSpeedMultiplier.Sort();
        }

        if (value > 200)
        {
            if (value > _highestEnergyMultiplier[0]) _highestEnergyMultiplier[0] = value-200;
            else _highestEnergyMultiplier.Add(value-200);
            _highestEnergyMultiplier.Sort();
        }
        if (value == 6767) _enableDrillPatch = true;
    }

    public void OnRemoveItem(InventoryItem item)
    {
        if (!Plugin.Multipliers.TryGetValue(item.techType, out float value)) return;
        
        if (value > 100 && value < 200)
        {
            _highestSpeedMultiplier.Remove(value-100);
            _highestSpeedMultiplier.Sort();
        }

        if (value > 200)
        {
            _highestEnergyMultiplier.Remove(value-200);
            _highestEnergyMultiplier.Sort();
        }
        if (value == 6767) _enableDrillPatch = true;
    }
    
    public float GetHighestSpeedMultiplier()
    {
        return _highestSpeedMultiplier[0];
    }

    public float GetHighestEnergyMultiplier()
    {
        return _highestEnergyMultiplier[0];
    }

    public bool EnableDrillPatch()
    {
        return _enableDrillPatch;
    }
}