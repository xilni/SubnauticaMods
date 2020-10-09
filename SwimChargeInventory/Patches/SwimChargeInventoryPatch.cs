using HarmonyLib;
using SMLHelper.V2.Handlers;
using System;
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

            // Check config to charge batteries
            var chargeBatteries = SwimChargeInventory.config.chargeBatteries;

            // Iterate through inventory looking for chargeables
            foreach (var item in Inventory.Get().container)
            {
                // Is item chargeable tool?
                if (item.item.gameObject.TryGetComponent(out EnergyMixin energyMixinComponent))
                {
                    // Does item need charging?
                    var battery = energyMixinComponent.GetBattery().GetComponent<IBattery>();
                    if (battery.charge < battery.capacity)
                    {
                        Console.WriteLine($"Charging {item.item.GetTechType()}");
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

        private static bool HeldToolNeedsCharge()
        {
            var heldTool = Inventory.main.GetHeldTool();
            var heldToolNeedsCharge = false;

            // If held item
            if (heldTool != null)
            {
                // Is held tool chargeable and below 100% charge?
                if (heldTool.gameObject.TryGetComponent(out EnergyMixin heldToolEnergyMixin) && (heldToolEnergyMixin.charge < heldToolEnergyMixin.capacity))
                {
                    heldToolNeedsCharge = true;
                }
            }

            return heldToolNeedsCharge;
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
