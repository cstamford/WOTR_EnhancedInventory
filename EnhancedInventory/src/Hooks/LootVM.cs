using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Items;
using Kingmaker.Items.Parts;
using Kingmaker.UI.MVVM._VM.Loot;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace EnhancedInventory.Hooks
{
    // This patch redirects the foreach loop to our function which filters each item based on settings.
    [HarmonyPatch(typeof(LootObjectVM), nameof(LootObjectVM.TransferAllItems))]
    public static class LootObjectVM_TransferAllItems
    {
        private static MethodInfo Method__FilterItem = AccessTools.Method(typeof(LootObjectVM_TransferAllItems), nameof(FilterItem));

        private static bool FilterItem(ItemEntity item)
        {
            if (!Main.Settings.EnableCollectAllTweaks) return true;

            if (Main.Settings.CollectAllZeroWeightItems && item.Blueprint.Weight <= 0.0f) return true;
            if (Main.Settings.CollectAllUnidentifiedItems && !item.IsIdentified) return true;
            if (Main.Settings.CollectAllWeightValue && item.Blueprint.Cost / item.Blueprint.Weight >= Main.Settings.CollectAllWeightValueCutoff) return true;

            CopyScroll scroll = item.Blueprint.GetComponent<CopyScroll>();
            CopyRecipe recipe = item.Blueprint.GetComponent<CopyRecipe>();
            ItemPartShowInfoCallback cb = item.Get<ItemPartShowInfoCallback>();
            bool is_copyable_scroll = scroll != null && Game.Instance.Player.Party.Any(i => scroll.CanCopy(item, i));
            bool is_unlearned_recipe = recipe != null && Game.Instance.Player.Party.Any(i => recipe.CanCopy(item, i));
            bool is_unread_document = cb != null && (!cb.m_Settings.Once || !cb.m_Triggered);

            return Main.Settings.CollectAllUsefulItems && (is_copyable_scroll || is_unlearned_recipe || is_unread_document);
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> il = instructions.ToList();

            for (int i = 0; i < il.Count; ++i)
            {
                if (il[i].opcode == OpCodes.Stloc_1)
                {
                    il.Insert(++i, new CodeInstruction(OpCodes.Ldloc_1)); // load ItemEntity
                    il.Insert(++i, new CodeInstruction(OpCodes.Call, Method__FilterItem)); // call FilterItem

                    // Scan back for the br.s, then with its label, insert a jump to next element if we're false.
                    for (int j = i - 2; j >= 0; --j)
                    {
                        if (il[j].opcode == OpCodes.Br_S)
                        {
                            il.Insert(++i, new CodeInstruction(OpCodes.Brfalse_S, il[j].operand)); // jump to next element
                            break;
                        }
                    }

                    break;
                }
            }

            return il.AsEnumerable();
        }
    }
}
