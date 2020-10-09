using HarmonyLib;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using SwimChargeInventory.Configuration;

namespace SwimChargeInventory
{
    [QModCore]
    public static class SwimChargeInventory
    {
        // 
        internal static Config config { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

        [QModPatch]
        public static void Load()
        {
            Harmony harmony = new Harmony("com.xilni.swimchargeinventory");
            harmony.PatchAll();
        }
    }
}
