using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;

namespace SwimChargeInventory.Configuration
{
    [Menu("SwimChargeInventory options")]
    public class Config : ConfigFile
    {
        [Toggle("Charge loose batteries in inventory")]
        public bool chargeBatteries = false;
    }
}
