using BepInEx;
using HarmonyLib;
using SMLHelper.V2.Handlers;
using SwimChargeInventory.Configuration;

namespace SwimChargeInventory
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class SwimChargeInventory : BaseUnityPlugin
    {
        internal const string MODNAME = "SwimChargeInventory";
        internal const string ASSEMBLY_TITLE = "SwimChargeInventory (BepInEx)";
        internal const string AUTHOR = "xilni";
        internal const string GUID = "com.xilni.swimchargeinventory";
        internal const string VERSION = "2.0.1.0";

        internal static Config config { get; } = OptionsPanelHandler.RegisterModOptions<Config>();
        internal static BepInEx.Logging.ManualLogSource logger;

        public void Start()
        {
            logger = Logger;
            logger.LogInfo("Loading patches.");

            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll();

            logger.LogInfo("Done loading all patches.");
        }
    }
}
