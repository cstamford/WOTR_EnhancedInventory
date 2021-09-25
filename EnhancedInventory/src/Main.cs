using EnhancedInventory.Events;
using EnhancedInventory.Settings;
using EnhancedInventory.Util;
using HarmonyLib;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace EnhancedInventory
{
#if DEBUG
    [EnableReloading]
#endif
    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static Data Settings;

        public static readonly RemappableInt FilterMapper = new RemappableInt();
        public static readonly RemappableInt SorterMapper = new RemappableInt();

        private static Harmony m_harmony;
        private static OnAreaLoad m_area_load_handler;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            Settings = Data.Load<Data>(modEntry);

            if (ModInterop.HasModConflicts())
            {
                Logger.Error("Loading failed due to detected mod conflicts.");
                return false;
            }

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

#if DEBUG
            modEntry.OnUnload = OnUnload;
#endif

            m_harmony = new Harmony(modEntry.Info.Id);
            m_harmony.PatchAll(Assembly.GetExecutingAssembly());

            m_area_load_handler = new OnAreaLoad();
            EventBus.Subscribe(m_area_load_handler);

            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry _)
        {
            GUILayout.Space(4);
            SettingsGUI.Draw();
            GUILayout.Space(4);
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Settings.Save(modEntry);
        }

        private static bool OnUnload(UnityModManager.ModEntry _)
        {
            m_harmony.UnpatchAll();
            EventBus.Unsubscribe(m_area_load_handler);
            return true;
        }

        public static void RefreshRemappers()
        {
            FilterMapper.Clear();
            SorterMapper.Clear();

            foreach (FilterCategories flag in EnumHelper.ValidFilterCategories)
            {
                if (Settings.FilterOptions.HasFlag(flag))
                {
                    FilterMapper.Add(FilterCategoryMap[flag].Item1);
                }
            }

            foreach (SorterCategories flag in EnumHelper.ValidSorterCategories)
            {
                if (Settings.SorterOptions.HasFlag(flag))
                {
                    SorterMapper.Add(SorterCategoryMap[flag].Item1);
                }
            }
        }

        public static readonly Dictionary<FilterCategories, Tuple<int, string>> FilterCategoryMap =
            new Dictionary<FilterCategories, Tuple<int, string>>
        {
            [FilterCategories.NoFilter]         = new Tuple<int, string>((int)ItemsFilter.FilterType.NoFilter, null),
            [FilterCategories.Weapon]           = new Tuple<int, string>((int)ItemsFilter.FilterType.Weapon, null),
            [FilterCategories.Armor]            = new Tuple<int, string>((int)ItemsFilter.FilterType.Armor, null),
            [FilterCategories.Accessories]      = new Tuple<int, string>((int)ItemsFilter.FilterType.Accessories, null),
            [FilterCategories.Ingredients]      = new Tuple<int, string>((int)ItemsFilter.FilterType.Ingredients, null),
            [FilterCategories.Usable]           = new Tuple<int, string>((int)ItemsFilter.FilterType.Usable, null),
            [FilterCategories.Notable]          = new Tuple<int, string>((int)ItemsFilter.FilterType.Notable, null),
            [FilterCategories.NonUsable]        = new Tuple<int, string>((int)ItemsFilter.FilterType.NonUsable, null),
            [FilterCategories.QuickslotUtils]   = new Tuple<int, string>((int)ExpandedFilterType.QuickslotUtilities, "Quickslot utilities"),
            [FilterCategories.UnlearnedScrolls] = new Tuple<int, string>((int)ExpandedFilterType.UnlearnedScrolls, "Unlearned scrolls"),
            [FilterCategories.UnlearnedRecipes] = new Tuple<int, string>((int)ExpandedFilterType.UnlearnedRecipes, "Unlearned recipes"),
            [FilterCategories.UnreadDocuments]  = new Tuple<int, string>((int)ExpandedFilterType.UnreadDocuments, "Unread documents"),
            [FilterCategories.UsableWithoutUMD] = new Tuple<int, string>((int)ExpandedFilterType.UsableWithoutUMD, "Usable without UMD check"),
        };

        public static readonly Dictionary<SorterCategories, Tuple<int, string>> SorterCategoryMap =
            new Dictionary<SorterCategories, Tuple<int, string>>
        {
            [SorterCategories.NotSorted]        = new Tuple<int, string>((int)ItemsFilter.SorterType.NotSorted, null),
            [SorterCategories.TypeUp]           = new Tuple<int, string>((int)ItemsFilter.SorterType.TypeUp, null),
            [SorterCategories.TypeDown]         = new Tuple<int, string>((int)ItemsFilter.SorterType.TypeDown, null),
            [SorterCategories.PriceUp]          = new Tuple<int, string>((int)ItemsFilter.SorterType.PriceUp, null),
            [SorterCategories.PriceDown]        = new Tuple<int, string>((int)ItemsFilter.SorterType.PriceDown, null),
            [SorterCategories.NameUp]           = new Tuple<int, string>((int)ItemsFilter.SorterType.NameUp, null),
            [SorterCategories.NameDown]         = new Tuple<int, string>((int)ItemsFilter.SorterType.NameDown, null),
            [SorterCategories.DateUp]           = new Tuple<int, string>((int)ItemsFilter.SorterType.DateUp, null),
            [SorterCategories.DateDown]         = new Tuple<int, string>((int)ItemsFilter.SorterType.DateDown, null),
            [SorterCategories.WeightUp]         = new Tuple<int, string>((int)ItemsFilter.SorterType.WeightUp, null),
            [SorterCategories.WeightDown]       = new Tuple<int, string>((int)ItemsFilter.SorterType.WeightDown, null),
            [SorterCategories.WeightValueUp]    = new Tuple<int, string>((int)ExpandedSorterType.WeightValueUp, "Price / Weight (in ascending order)"),
            [SorterCategories.WeightValueDown]  = new Tuple<int, string>((int)ExpandedSorterType.WeightValueDown, "Price / Weight (in descending order)")
        };
    }
}
