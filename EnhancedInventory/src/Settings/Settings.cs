using System;
using UnityEngine;
using UnityModManagerNet;

namespace EnhancedInventory.Settings
{
    [Flags]
    public enum InventorySearchCriteria
    {
        ItemName        = 1 << 0,
        ItemType        = 1 << 1,
        ItemSubtype     = 1 << 2,
        ItemDescription = 1 << 3,

        Default = ItemName | ItemType | ItemSubtype
    }

    [Flags]
    public enum SpellbookSearchCriteria
    {
        SpellName        = 1 << 0,
        SpellDescription = 1 << 1,
        SpellSaves       = 1 << 2,
        SpellSchool      = 1 << 3,

        Default = SpellName | SpellSaves | SpellSchool,
    }

    [Flags]
    public enum HighlightLootableOptions
    {
        UnlearnedScrolls    = 1 << 0,
        UnlearnedRecipes    = 1 << 1,
        UnreadDocuments     = 1 << 2,

        Default = UnlearnedScrolls | UnlearnedRecipes | UnreadDocuments
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

        Default = Weapon |
            Armor |
            Accessories |
            Ingredients |
            Usable |
            Notable |
            NonUsable |
            QuickslotUtils |
            UnlearnedScrolls |
            UnlearnedRecipes |
            UnreadDocuments |
            UsableWithoutUMD
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
        WeightValueDown = 1 << 11,

        Default = TypeUp |
            PriceDown |
            DateDown |
            WeightDown |
            WeightValueUp
    }

    // TODO BEFORE RELEASE: Add filter categories

    public class Data : UnityModManager.ModSettings
    {
        public bool EnableInventorySearchBar = true;
        public bool EnableSpellbookSearchBar = true;
        public bool EnableHighlightableLoot = true;
        public bool EnableVisualOverhaulSorting = true;

        public bool InventorySearchBarResetFilterWhenOpening = false;
        public bool InventorySearchBarFocusWhenOpening = true;
        public bool InventorySearchBarScrollResetOnSubmit = true;
        public bool InventorySearchBarEnableCategoryButtons = false;

        public bool SpellbookSearchBarFocusWhenOpening = true;
        public bool SpellbookShowAllSpellsByDefault = true;
        public bool SpellbookShowMetamagicByDefault = true;
        public bool SpellbookShowLevelWhenViewingAllSpells = true;
        public bool SpellbookHideEmptyMetamagicCircles = true;

        public InventorySearchCriteria InventorySearchCriteria = InventorySearchCriteria.Default;
        public SpellbookSearchCriteria SpellbookSearchCriteria = SpellbookSearchCriteria.Default;
        public FilterCategories FilterOptions = FilterCategories.Default;
        public SorterCategories SorterOptions = SorterCategories.Default;
        public HighlightLootableOptions HighlightLootOptions = HighlightLootableOptions.Default;

        public Color32 HighlightLootBorder = new Color32(255, 215, 0, 255);
        public Color32 HighlightLootBackground = new Color32(255, 215, 0, 255);

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}
