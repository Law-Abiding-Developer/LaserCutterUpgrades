using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using UpgradesLIB.Items.Equipment;

namespace LaserCutterUpgrades;

public class LaserCutterUpgrade
{
    public PrefabInfo Info { get; set; }
    public TechType TechType { get; set; }
    public string ClassID { get; set; }

    public LaserCutterUpgrade(PrefabInfo info)
    {
        Info = info;
        TechType = info.TechType;
        ClassID = info.ClassID;
    }

    public void Register(List<Ingredient> ingredients)
    {
        var prefab = new CustomPrefab(Info);
        var clone = new CloneTemplate(Info, TechType.CyclopsShieldModule);
        clone.ModifyPrefab = obj =>
        {
            obj.transform.localScale /= 1.5f;
        };
        prefab.SetGameObject(clone);
        prefab.SetRecipe(new RecipeData(ingredients))
            .WithFabricatorType(Handheldprefab.HandheldfabTreeType)
            .WithStepsToFabricatorTab("Tools", "LaserCutterTab")
            .WithCraftingTime(5f);
        prefab.SetUnlock(TechType.LaserCutter);
        prefab.SetEquipment(Plugin.EquipmentType);
        prefab.SetPdaGroupCategory(UpgradesLIB.Plugin.toolupgrademodules, Plugin.LaserCutterUpgrades);
        prefab.Register();
        Plugin.Logger.LogInfo($"Prefab {TechType} has been registered!");
    }
}