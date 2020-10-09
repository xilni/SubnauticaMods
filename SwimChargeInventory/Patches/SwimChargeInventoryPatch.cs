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
            // 
            if (Player.main.IsUnderwater() && Player.main.gameObject.GetComponent<Rigidbody>().velocity.magnitude > 2f)
            {
                bool swimChargeFinsEquipped = false;

                // Check if swim charge or ultra glide swim charge fins equipped
                if (TechTypeHandler.TryGetModdedTechType("ugscfins", out TechType ultraGlideSwimChargeTechType))
                {
                    swimChargeFinsEquipped = (Inventory.Get().equipment.GetCount(TechType.SwimChargeFins) + Inventory.Get().equipment.GetCount(ultraGlideSwimChargeTechType)) > 0;
                }
                else
                {
                    swimChargeFinsEquipped = Inventory.Get().equipment.GetCount(TechType.SwimChargeFins) > 0;
                }

                if (swimChargeFinsEquipped)
                {
                    // Check if already holding item and it needs a charge
                    PlayerTool heldTool = Inventory.main.GetHeldTool();
                    bool heldToolNeedsCharge = false;
                    if (heldTool != null)
                    {
                        // Is held tool chargeable and below 100% charge?
                        if (heldTool.gameObject.TryGetComponent<EnergyMixin>(out EnergyMixin heldToolEnergyMixin) && (heldToolEnergyMixin.charge < heldToolEnergyMixin.capacity))
                        {
                            heldToolNeedsCharge = true;
                        }
                    }

                    // If no held tool or if it's fully charged, search for and charge an item
                    if (heldTool == null || !heldToolNeedsCharge)
                    {
                        // Iterate through inventory looking for chargeables
                        foreach (var item in Inventory.Get().container)
                        {
                            // Is item chargeable?
                            if (item.item.gameObject.TryGetComponent<EnergyMixin>(out EnergyMixin energyMixinComponent)) {

                                // Does item need charging?
                                var battery = energyMixinComponent.GetBattery().GetComponent<IBattery>();
                                if (battery.charge < battery.capacity)
                                {
                                    // Add some charge
                                    battery.charge += 0.005f;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
