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
                    // Check if holding item and it needs a charge
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

                    Console.WriteLine($"[SwimChargeInventory] heldToolNeedsCharge {heldToolNeedsCharge}");

                    // If no held tool or if it's fully charged, search for and charge an item
                    if (heldTool == null || !heldToolNeedsCharge)
                    {
                        Console.WriteLine("[SwimChargeInventory] In charging loop!");

                        // Iterate through inventory looking for chargeables
                        foreach (var item in Inventory.Get().container)
                        {
                            // If chargeable
                            if (item.item.TryGetComponent<EnergyMixin>(out EnergyMixin energyMixin))
                            {
                                Console.WriteLine($"[SwimChargeInventory] Found chargeable {item.item.GetTechType()}: {energyMixin.charge} / {energyMixin.capacity}");

                                // If not fully charged, addEnergy
                                if (energyMixin.charge < energyMixin.capacity)
                                {
                                    energyMixin.AddEnergy(0.005f);
                                    Console.WriteLine($"[SwimChargeInventory] Adding energy to {item.item.GetTechType()}");
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