using System;
using System.Collections.Generic;
using UnityEngine;
using UpgradesLIB;

namespace LaserCutterUpgrades;

public class LaserCutterUpgradeInput : ModdedUpgradeConsoleInput
{
    private readonly List<float> _highestSpeedMultiplier = new();
    private readonly List<float> _highestEnergyMultiplier = new();
    private bool _enableDrillPatch;
    
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
        
        if (value is > 100 and < 200)
        {
            _highestSpeedMultiplier.Add(value-100);
            _highestSpeedMultiplier.Sort((x, y) => y.CompareTo(x));
        }
        else if (Mathf.Approximately(value, 6767)) _enableDrillPatch = true;
        else if (value > 200)
        {
            _highestEnergyMultiplier.Add(value-200);
            _highestEnergyMultiplier.Sort((x, y) => y.CompareTo(x));
        }
    }

    private float _timer = 0;
    public void Update()
    {
        if (_timer >= 30)
        {
            _highestEnergyMultiplier.Sort((x, y) => y.CompareTo(x));
            _highestSpeedMultiplier.Sort((x, y) => y.CompareTo(x));
            _timer = 0;
        }
        _timer += Time.deltaTime;
    }

    public void OnRemoveItem(InventoryItem item)
    {
        if (!Plugin.Multipliers.TryGetValue(item.techType, out float value)) return;
        
        if (value is > 100 and < 200)
        {
            _highestSpeedMultiplier.Remove(value-100);
            _highestSpeedMultiplier.Sort((x, y) => y.CompareTo(x));
        }
        else if (Mathf.Approximately(value, 6767)) _enableDrillPatch = false;
        else if (value > 200)
        {
            _highestEnergyMultiplier.Remove(value-200);
            _highestEnergyMultiplier.Sort((x, y) => y.CompareTo(x));
        }
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