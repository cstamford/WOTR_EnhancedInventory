using EnhancedInventory.Settings;
using EnhancedInventory.Util;
using HarmonyLib;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._PCView.Slots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UniRx;

namespace EnhancedInventory.Hooks
{
    // Handles both adding selected sorters to the sorter dropdowns and making sure that the dropdown is properly updates to match the selected sorter.
    [HarmonyPatch(typeof(ItemsFilterPCView))]
    public static class ItemsFilterPCView_
    {
        private static MethodInfo m_set_dropdown = AccessTools.Method(typeof(ItemsFilterPCView_), nameof(SetDropdown));
        private static MethodInfo m_set_sorter = AccessTools.Method(typeof(ItemsFilterPCView_), nameof(SetSorter));

        private static void SetDropdown(ItemsFilterPCView instance, ItemsFilter.SorterType val)
        {
            instance.m_Sorter.value = Main.SorterMapper.From((int)val);
        }

        private static void SetSorter(ItemsFilterPCView instance, int val)
        {
            instance.ViewModel.SetCurrentSorter((ItemsFilter.SorterType)Main.SorterMapper.To(val));
        }

        // In BindViewImplementation, there are two inline delegates; we replace both of those in order with our own.
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(ItemsFilterPCView.BindViewImplementation))]
        public static IEnumerable<CodeInstruction> BindViewImplementation(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> il = instructions.ToList();

            int ldftn_count = 0;

            for (int i = 0; i < il.Count && ldftn_count < 2; ++i)
            {
                if (il[i].opcode == OpCodes.Ldftn)
                {
                    il[i].operand = ldftn_count++ == 0 ? m_set_dropdown : m_set_sorter;
                }
            }

            return il.AsEnumerable();
        }

        // Adds the sorters to the dropdown.
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ItemsFilterPCView.Initialize), new Type[] { })]
        public static void Initialize(ItemsFilterPCView __instance)
        {
            __instance.m_Sorter.ClearOptions();

            List<string> options = new List<string>();

            foreach (SorterCategories flag in EnumHelper.ValidSorterCategories)
            {
                if (Main.Settings.SorterOptions.HasFlag(flag))
                {
                    (int idx, string text) = Main.SorterCategoryMap[flag];

                    if (text == null)
                    {
                        text = LocalizedTexts.Instance.ItemsFilter.GetText((ItemsFilter.SorterType)idx);
                        Main.SorterCategoryMap[flag] = (idx, text);
                    }

                    options.Add(text);
                }
            }

            __instance.m_Sorter.AddOptions(options);
        }
    }
}
