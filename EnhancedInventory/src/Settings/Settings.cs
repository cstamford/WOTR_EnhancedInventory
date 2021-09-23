using System;
using UnityEngine;
using UnityModManagerNet;

namespace EnhancedInventory.Settings
{
    [Flags]
    public enum SearchBarOptions
    {
        None            = 0,
        ItemName        = 1 << 0,
        ItemType        = 1 << 1,
        ItemSubtype     = 1 << 2,
        ItemDescription = 1 << 3,
    }

    [Flags]
    public enum HighlightLootableOptions
    {
        None                = 0,
        UnlearnedScrolls    = 1 << 0,
        UnlearnedRecipes    = 1 << 1,
        UnreadDocuments     = 1 << 2
    }

    [Flags]
    public enum FilterCategories
    {
        NoFilter            = 0,
        Weapon              = 1 << 0,
        Armor               = 1 << 1,
        Accessories         = 1 << 2,
        Ingredients         = 1 << 3,
        Usable              = 1 << 4,
        Notable             = 1 << 5,
        NonUsable           = 1 << 6,
        QuickslotUtils      = 1 << 7,
        UnlearnedScrolls    = 1 << 8,
        UnlearnedRecipes    = 1 << 9,
        UnreadDocuments     = 1 << 10,
        UsableWithoutUMD    = 1 << 11,
    }

    [Flags]
    public enum SorterCategories
    {
        NotSorted       = 0,
        TypeUp          = 1 << 0,
        TypeDown        = 1 << 1,
        PriceUp         = 1 << 2,
        PriceDown       = 1 << 3,
        NameUp          = 1 << 4,
        NameDown        = 1 << 5,
        DateUp          = 1 << 6,
        DateDown        = 1 << 7,
        WeightUp        = 1 << 8,
        WeightDown      = 1 << 9,
        WeightValueUp   = 1 << 10,
        WeightValueDown = 1 << 11
    }

    // TODO BEFORE RELEASE: Add filter categories

    public class Data : UnityModManager.ModSettings
    {
        public bool EnableSearchBar = true;
        public bool EnableHighlightableLoot = true;
        public bool EnableVisualOverhaulSorting = true;

        public bool SearchBarEnableCategoryButtons = false;
        public bool SearchBarResetFilterWhenOpeningInv = false;

        public SearchBarOptions SearchBarOptions =
            SearchBarOptions.ItemName |
            SearchBarOptions.ItemType |
            SearchBarOptions.ItemSubtype;

        public HighlightLootableOptions HighlightLootOptions =
            HighlightLootableOptions.UnlearnedScrolls |
            HighlightLootableOptions.UnlearnedRecipes |
            HighlightLootableOptions.UnreadDocuments;

        public Color32 HighlightLootBorder = new Color32(255, 215, 0, 255);
        public Color32 HighlightLootBackground = new Color32(255, 215, 0, 255);

        public FilterCategories FilterOptions =
            FilterCategories.Weapon |
            FilterCategories.Armor |
            FilterCategories.Accessories |
            FilterCategories.Ingredients |
            FilterCategories.Usable |
            FilterCategories.Notable |
            FilterCategories.NonUsable |
            FilterCategories.QuickslotUtils |
            FilterCategories.UnlearnedScrolls |
            FilterCategories.UnlearnedRecipes |
            FilterCategories.UnreadDocuments |
            FilterCategories.UsableWithoutUMD;

        public SorterCategories SorterOptions =
            SorterCategories.TypeUp |
            SorterCategories.PriceDown |
            SorterCategories.DateDown |
            SorterCategories.WeightDown |
            SorterCategories.WeightValueUp;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}
