using EnhancedInventory.Settings;
using HarmonyLib;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._PCView.Slots;
using Owlcat.Runtime.UniRx;
using System;
using System.Collections.Generic;
using UniRx;

namespace EnhancedInventory.Hooks
{
    // TODO docs
    [HarmonyPatch(typeof(ItemsFilterPCView))]
    public static class ItemsFilterPCView_Initialize
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ItemsFilterPCView.BindViewImplementation))]
        public static bool BindViewImplementation(ItemsFilterPCView __instance)
        {
            // TODO: Transpiler? Reverse patch? I don't like this...
            // It would be nice if we could easily patch the delegates.

            __instance.Show();
            __instance.SubscribeToggles();
            __instance.AddDisposable(__instance.ViewModel.CurrentSorter.Subscribe(delegate (ItemsFilter.SorterType value)
            {
                __instance.m_Sorter.value = Main.SorterMapper.From((int)value);
            }));
            __instance.AddDisposable(__instance.m_Sorter.OnValueChangedAsObservable().Subscribe(delegate (int value)
            {
                __instance.ViewModel.SetCurrentSorter((ItemsFilter.SorterType)Main.SorterMapper.To(value));
            }));
            __instance.SetTooltips();

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(ItemsFilterPCView.Initialize))]
        public static void Initialize(ItemsFilterPCView __instance)
        {
            __instance.m_Sorter.ClearOptions();

            List<string> options = new List<string>();

            foreach (SorterCategories flag in Enum.GetValues(typeof(SorterCategories)))
            {
                if (Main.Settings.SorterOptions.HasFlag(flag))
                {
                    (int idx, string text) = Main.SorterCategoryMap[flag];

                    if (text == null)
                    {
                        text = LocalizedTexts.Instance.ItemsFilter.GetText((ItemsFilter.SorterType)idx);
                        Main.SorterCategoryMap[flag] = new Tuple<int, string>(idx, text);
                    }

                    options.Add(text);
                }
            }

            __instance.m_Sorter.AddOptions(options);
        }
    }
}