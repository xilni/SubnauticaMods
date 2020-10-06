using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace PrawnSolar.Patches
{
    [HarmonyPatch(typeof(Exosuit), "Update")]
    class PrawnSolarModuleUpdatePatch
    {
        // Constants
        private const float maxSolarDepth = 200f;

        // Grab AddEnergy method using reflection
        static MethodInfo addEnergyMethod = typeof(Vehicle).GetMethod("AddEnergy", BindingFlags.NonPublic | BindingFlags.Instance);

        static void Prefix(Exosuit __instance)
        {
            // Get module count
            var count = __instance.modules.GetCount(PrawnSolar.prawnSolarModule.TechType);

            // If equipped, proceed
            if (count > 0)
            {
                // Determine light value
                DayNightCycle main = DayNightCycle.main;
                if (main == null)
                {
                    return;
                }

                float num = Mathf.Clamp01((maxSolarDepth + __instance.transform.position.y) / maxSolarDepth);
                float localLightScalar = main.GetLocalLightScalar();
                float amount = 1f * localLightScalar * num * (float)count;

                // Add energy to vehicle
                addEnergyMethod.Invoke(__instance, new object[] { amount * main.deltaTime });
            }
        }
    }
}
