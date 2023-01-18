using HarmonyLib;
using BepInEx;

namespace PrawnSolar
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class PrawnSolar : BaseUnityPlugin
    {
        internal const string MODNAME = "PrawnSolar";
        internal const string ASSEMBLY_TITLE = "PrawnSolar (BepInEx)";
        internal const string AUTHOR = "xilni";
        internal const string GUID = "com.xilni.prawnsolarmodule";
        internal const string VERSION = "2.0.0.0";

        internal static BepInEx.Logging.ManualLogSource logger;

        internal static Modules.PrawnSolarModule prawnSolarModule = new Modules.PrawnSolarModule();

        public void Start()
        {
            logger = Logger;
            logger.LogInfo("Loading patches.");

            prawnSolarModule.Patch();

            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll();

            logger.LogInfo("Done loading all patches.");
        }
    }
}
