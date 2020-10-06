using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;

namespace PrawnSolar.Modules
{
    class PrawnSolarModule : Equipable
    {
        public PrawnSolarModule() : base(
            "PrawnSolarModule",
            "Prawn solar charger",
            "Recharges Prawn Suit power cells in sunlight.")
        {
        }

        public override EquipmentType EquipmentType => EquipmentType.ExosuitModule;
        public override TechType RequiredForUnlock => TechType.BaseUpgradeConsole;
        public override TechGroup GroupForPDA => TechGroup.VehicleUpgrades;
        public override TechCategory CategoryForPDA => TechCategory.VehicleUpgrades;
        public override CraftTree.Type FabricatorType => CraftTree.Type.SeamothUpgrades;
        public override string[] StepsToFabricatorTab => new string[] { "ExosuitModules" };
        public override QuickSlotType QuickSlotType => QuickSlotType.Passive;

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(TechType.EnameledGlass, 1)
                }
            };
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return SpriteManager.Get(TechType.SeamothSolarCharge);
        }
    }
}
