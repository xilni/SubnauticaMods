using HarmonyLib;
using QModManager.API.ModLoading;
using System;

namespace SwimChargeInventory
{
    [QModCore]
    public static class SwimChargeInventory
    {

        [QModPatch]
        public static void Load()
        {
            Console.WriteLine("[SwimChargeInventory] Starting up.");

            Harmony harmony = new Harmony("com.xilni.swimchargeinventory");
            harmony.PatchAll();

            Console.WriteLine("[SwimChargeInventory] Patched.");
        }
    }
}
