using HarmonyLib;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace SwimChargeInventory.Patches
{
    [HarmonyPatch(typeof(UpdateSwimCharge), "FixedUpdate")]
    class SwimChargeInventoryPatch
    {
        static void Prefix(UpdateSwimCharge __instance)
        {
            // If not underwater or moving quick enough, return early
            if (!Player.main.IsUnderwater() || Player.main.gameObject.GetComponent<Rigidbody>().velocity.magnitude <= 2f)
            {
                return;
            }

            // If no swim fins equipped, return early
            if (!SwimChargeFinsEquipped())
            {
                return;
            }

            // If held tool needs charge, return early and let unpatched method handle it
            if (HeldToolNeedsCharge())
            {
                return;
            }

            // Iterate through inventory looking for chargeables
            SearchInventoryAndCharge();
        }

        private static bool HeldToolNeedsCharge()
        {
            var heldTool = Inventory.main.GetHeldTool();

            // Is held tool chargeable and below 100% charge?
            if (heldTool && heldTool.gameObject.TryGetComponent(out EnergyMixin heldToolEnergyMixin) && (heldToolEnergyMixin.charge < heldToolEnergyMixin.capacity))
            {
                return true;
            }

            return false;
        }

        private static void SearchInventoryAndCharge()
        {
            // Check config to charge batteries
            var chargeBatteries = SwimChargeInventory.config.chargeBatteries;

            foreach (var item in Inventory.Get().container)
            {
                // Is item chargeable tool?
                if (item.item.gameObject.TryGetComponent(out EnergyMixin energyMixinComponent))
                {
                    // Does item need charging?
                    var battery = energyMixinComponent.GetBattery();
                    if (battery.charge < battery.capacity)
                    {
                        // Add some charge
                        battery.charge += 0.005f;
                        break;
                    }
                }

                // If we're charging batteries
                if (chargeBatteries &&
                    item.item.TryGetComponent<IBattery>(out IBattery ibatteryComponent) &&
                    ibatteryComponent.charge < ibatteryComponent.capacity)
                {
                    // Add some charge
                    ibatteryComponent.charge += 0.005f;
                    break;
                }
            }
        }

        private static bool SwimChargeFinsEquipped()
        {
            // Check if swim charge or ultra glide swim charge fins equipped (if MoreModifiedItems mod present)
            return TechTypeHandler.TryGetModdedTechType("ugscfins", out TechType ultraGlideSwimChargeTechType)
                ? (Inventory.Get().equipment.GetCount(TechType.SwimChargeFins) + Inventory.Get().equipment.GetCount(ultraGlideSwimChargeTechType)) > 0
                : Inventory.Get().equipment.GetCount(TechType.SwimChargeFins) > 0;
        }
    }
}
