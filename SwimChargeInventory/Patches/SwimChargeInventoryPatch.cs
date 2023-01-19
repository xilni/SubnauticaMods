using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System;
using System.Linq;

namespace SwimChargeInventory.Patches
{
    [HarmonyPatch(typeof(UpdateSwimCharge), nameof(UpdateSwimCharge.FixedUpdate))]
    class SwimChargeInventoryPatch
    {
        public static readonly MethodInfo funcGetHeldTool = AccessTools.Method(typeof(Inventory), nameof(Inventory.GetHeldTool));
        public static readonly MethodInfo funcEnergyMixin = AccessTools.Method(typeof(GameObject), nameof(GameObject.GetComponent), null, new Type[] { typeof(EnergyMixin) });
        public static readonly MethodInfo funcAddEnergy = AccessTools.Method(typeof(EnergyMixin), nameof(EnergyMixin.AddEnergy));
        public static readonly MethodInfo funcDeltaTime = AccessTools.PropertyGetter(typeof(Time), nameof(Time.deltaTime));
        public static readonly FieldInfo fieldChargePerSecond = AccessTools.Field(typeof(UpdateSwimCharge), nameof(UpdateSwimCharge.chargePerSecond));
        public static readonly MethodInfo funcChargeInventory = AccessTools.Method(typeof(SwimChargeInventoryPatch), nameof(SwimChargeInventoryPatch.SearchInventoryAndCharge));

        private static bool IsCode(CodeInstruction code, OpCode opcode)
        {
            if (opcode.Equals(OpCodes.Ldarg) || opcode.Equals(OpCodes.Ldarg_S))
            {
                return code.opcode.Equals(OpCodes.Ldarg_0) || code.opcode.Equals(OpCodes.Ldarg_1) || code.opcode.Equals(OpCodes.Ldarg_2) || code.opcode.Equals(OpCodes.Ldarg_3)
                    || code.opcode.Equals(OpCodes.Ldarg) || code.opcode.Equals(OpCodes.Ldarg_S);
            }
            if (opcode.Equals(OpCodes.Ldloc) || opcode.Equals(OpCodes.Ldloc_S))
            {
                return code.opcode.Equals(OpCodes.Ldloc_0) || code.opcode.Equals(OpCodes.Ldloc_1) || code.opcode.Equals(OpCodes.Ldloc_2) || code.opcode.Equals(OpCodes.Ldloc_3)
                    || code.opcode.Equals(OpCodes.Ldloc) || code.opcode.Equals(OpCodes.Ldloc_S);
            }
            if (opcode.Equals(OpCodes.Stloc) || opcode.Equals(OpCodes.Stloc_S))
            {
                return code.opcode.Equals(OpCodes.Stloc_0) || code.opcode.Equals(OpCodes.Stloc_1) || code.opcode.Equals(OpCodes.Stloc_2) || code.opcode.Equals(OpCodes.Stloc_3)
                    || code.opcode.Equals(OpCodes.Stloc) || code.opcode.Equals(OpCodes.Stloc_S);
            }
            return code.opcode.Equals(opcode);
        }
        private static bool IsCode(CodeInstruction orig, OpCode opcode, object operand)
        {
            return orig.opcode.Equals(opcode) && orig.operand.Equals(operand);
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UpdateSwimCharge_FixedUpdate_Transpiler(MethodBase original, ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            bool injected1 = false;
            bool injected2 = false;
            bool injected3 = false;
            var labelChargedHeldTool = generator.DefineLabel();
            var labelTestInventory = generator.DefineLabel();
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 5; i < codes.Count - 3; i++)
            {
                if (!injected1 &&
                    codes[i - 5].opcode.Equals(OpCodes.Callvirt) && codes[i - 5].operand.Equals(funcGetHeldTool) &&
                    IsCode(codes[i - 4], OpCodes.Stloc) &&
                    IsCode(codes[i - 3], OpCodes.Ldloc) &&
                    codes[i - 2].opcode.Equals(OpCodes.Ldnull) &&
                    codes[i - 1].opcode.Equals(OpCodes.Call) &&  // Inequality
                    codes[i].opcode.Equals(OpCodes.Brfalse))
                {
                    injected1 = true;
                    codes[i].operand = labelTestInventory;
                }
                if (!injected2 &&
                    codes[i - 5].opcode.Equals(OpCodes.Callvirt) && codes[i - 5].operand.Equals(funcEnergyMixin) &&
                    IsCode(codes[i - 4], OpCodes.Stloc) &&
                    IsCode(codes[i - 3], OpCodes.Ldloc) &&
                    codes[i - 2].opcode.Equals(OpCodes.Ldnull) &&
                    codes[i - 1].opcode.Equals(OpCodes.Call) &&  // Inequality
                    codes[i].opcode.Equals(OpCodes.Brfalse))
                {
                    injected2 = true;
                    codes[i].operand = labelTestInventory;
                }
                if (!injected3 && codes[i].opcode.Equals(OpCodes.Callvirt) && codes[i].operand.Equals(funcAddEnergy))
                {
                    injected3 = true;
                    codes[i + 2].labels = new List<Label>() { labelChargedHeldTool };
                    List<CodeInstruction> addition = new List<CodeInstruction>() {
                        new CodeInstruction(OpCodes.Brtrue_S, labelChargedHeldTool),  // If charged held tool, skip
                        new CodeInstruction(OpCodes.Ldarg_0 ) {labels = new List<Label>() { labelTestInventory } },  // Where to jump to test the inventory
                        new CodeInstruction(OpCodes.Ldfld, fieldChargePerSecond ),
                        new CodeInstruction(OpCodes.Call, funcDeltaTime ),
                        new CodeInstruction(OpCodes.Mul ),
                        new CodeInstruction(OpCodes.Call, funcChargeInventory),
                    };
                    codes.InsertRange(i + 1, addition);
                    break;
                }
            }

            if (!injected1) SwimChargeInventory.logger.LogError("Failed to apply patch 1 to UpdateSwimCharge_FixedUpdate_patch in SwimChargeInventoryPatch.");
            if (!injected2) SwimChargeInventory.logger.LogError("Failed to apply patch 2 to UpdateSwimCharge_FixedUpdate_patch in SwimChargeInventoryPatch.");
            if (!injected3) SwimChargeInventory.logger.LogError("Failed to apply patch 3 to UpdateSwimCharge_FixedUpdate_patch in SwimChargeInventoryPatch.");

            return codes.AsEnumerable();
        }

        private static bool SearchInventoryAndCharge(float amount)
        {
            foreach (InventoryItem inventoryItem in ((IEnumerable<InventoryItem>)Inventory.Get().container))
            {
                // Is item chargeable tool?
                if (inventoryItem.item.gameObject.TryGetComponent(out EnergyMixin energyMixinComponent) && energyMixinComponent.charge < energyMixinComponent.capacity)
                {
                    energyMixinComponent.AddEnergy(amount); 
                    return true;
                }

                // If charging batteries and we find a battery
                if (SwimChargeInventory.config.chargeBatteries && inventoryItem.item.TryGetComponent<IBattery>(out IBattery battery) && battery.charge < battery.capacity)
                {
                    battery.charge += amount;
                    return true;
                }
            }
            return false;
        }
    }
}
