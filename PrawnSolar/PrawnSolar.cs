using HarmonyLib;
using QModManager.API.ModLoading;
using System;

namespace PrawnSolar
{
    [QModCore]
    public static class PrawnSolar
    {
        internal static Modules.PrawnSolarModule prawnSolarModule = new Modules.PrawnSolarModule();

        [QModPatch]
        public static void Load()
        {
            Console.WriteLine("[PrawnSolar] Starting up.");

            prawnSolarModule.Patch();

            Harmony harmony = new Harmony("com.xilni.prawnsolarmodule");
            harmony.PatchAll();

            Console.WriteLine("[PrawnSolar] Patched.");
        }
    }
}
