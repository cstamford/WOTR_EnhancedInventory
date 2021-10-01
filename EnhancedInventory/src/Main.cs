using EnhancedInventory.Events;
using EnhancedInventory.Localization;
using EnhancedInventory.Settings;
using EnhancedInventory.Util;
using HarmonyLib;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.Common;
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
                if (Settings.FilterOptions.HasFlag(flag) || !Settings.EnableInventorySearchBar)
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

        public static readonly Dictionary<FilterCategories, (int, string)> FilterCategoryMap = new Dictionary<FilterCategories, (int, string)>
        {
            [FilterCategories.NoFilter]         = ((int)ItemsFilter.FilterType.NoFilter, null),
            [FilterCategories.Weapon]           = ((int)ItemsFilter.FilterType.Weapon, null),
            [FilterCategories.Armor]            = ((int)ItemsFilter.FilterType.Armor, null),
            [FilterCategories.Accessories]      = ((int)ItemsFilter.FilterType.Accessories, null),
            [FilterCategories.Ingredients]      = ((int)ItemsFilter.FilterType.Ingredients, null),
            [FilterCategories.Usable]           = ((int)ItemsFilter.FilterType.Usable, null),
            [FilterCategories.Notable]          = ((int)ItemsFilter.FilterType.Notable, null),
            [FilterCategories.NonUsable]        = ((int)ItemsFilter.FilterType.NonUsable, null),
            [FilterCategories.QuickslotUtils]   = ((int)ExpandedFilterType.QuickslotUtilities, InventoryStrings.QuickslotUtilities),
            [FilterCategories.UnlearnedScrolls] = ((int)ExpandedFilterType.UnlearnedScrolls, InventoryStrings.UnlearnedScrolls),
            [FilterCategories.UnlearnedRecipes] = ((int)ExpandedFilterType.UnlearnedRecipes, InventoryStrings.UnlearnedRecipes),
            [FilterCategories.UnreadDocuments]  = ((int)ExpandedFilterType.UnreadDocuments, InventoryStrings.UnreadDocuments),
            [FilterCategories.UsableWithoutUMD] = ((int)ExpandedFilterType.UsableWithoutUMD, InventoryStrings.UsableWithoutUMDCheck),
            [FilterCategories.CurrentEquipped]  = ((int)ExpandedFilterType.CurrentEquipped, InventoryStrings.CurrentEquipped),
            [FilterCategories.NonZeroPW]        = ((int)ExpandedFilterType.NonZeroPW, InventoryStrings.NonZeroPW),
        };

        public static readonly Dictionary<SorterCategories, (int, string)> SorterCategoryMap = new Dictionary<SorterCategories, (int, string)>
        {
            [SorterCategories.NotSorted]        = ((int)ItemsFilter.SorterType.NotSorted, null),
            [SorterCategories.TypeUp]           = ((int)ItemsFilter.SorterType.TypeUp, null),
            [SorterCategories.TypeDown]         = ((int)ItemsFilter.SorterType.TypeDown, null),
            [SorterCategories.PriceUp]          = ((int)ItemsFilter.SorterType.PriceUp, null),
            [SorterCategories.PriceDown]        = ((int)ItemsFilter.SorterType.PriceDown, null),
            [SorterCategories.NameUp]           = ((int)ItemsFilter.SorterType.NameUp, null),
            [SorterCategories.NameDown]         = ((int)ItemsFilter.SorterType.NameDown, null),
            [SorterCategories.DateUp]           = ((int)ItemsFilter.SorterType.DateUp, null),
            [SorterCategories.DateDown]         = ((int)ItemsFilter.SorterType.DateDown, null),
            [SorterCategories.WeightUp]         = ((int)ItemsFilter.SorterType.WeightUp, null),
            [SorterCategories.WeightDown]       = ((int)ItemsFilter.SorterType.WeightDown, null),
            [SorterCategories.WeightValueUp]    = ((int)ExpandedSorterType.WeightValueUp, InventoryStrings.PriceWeightAscending),
            [SorterCategories.WeightValueDown]  = ((int)ExpandedSorterType.WeightValueDown, InventoryStrings.PriceWeightDescending)
        };
    }
}
