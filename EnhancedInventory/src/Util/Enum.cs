using EnhancedInventory.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EnhancedInventory.Util
{
    public enum ExpandedFilterType
    {
        QuickslotUtilities = 8,
        UnlearnedScrolls = 9,
        UnlearnedRecipes = 10,
        UnreadDocuments = 11,
        UsableWithoutUMD = 12,
    }

    public enum ExpandedSorterType
    {
        WeightValueUp = 11,
        WeightValueDown = 12
    }

    public enum SpellbookFilter
    {
        NoFilter,
        TargetsFortitude,
        TargetsReflex,
        TargetsWill,
        SpellLevel0,
        SpellLevel1,
        SpellLevel2,
        SpellLevel3,
        SpellLevel4,
        SpellLevel5,
        SpellLevel6,
        SpellLevel7,
        SpellLevel8,
        SpellLevel9,
    }

    public static class EnumHelper
    {
        public static IEnumerable<InventorySearchCriteria> ValidInventorySearchCriteria
            = Enum.GetValues(typeof(InventorySearchCriteria)).Cast<InventorySearchCriteria>().Where(i => i != InventorySearchCriteria.Default);

        public static IEnumerable<SpellbookSearchCriteria> ValidSpellbookSearchCriteria
            = Enum.GetValues(typeof(SpellbookSearchCriteria)).Cast<SpellbookSearchCriteria>().Where(i => i != SpellbookSearchCriteria.Default);

        public static IEnumerable<HighlightLootableOptions> ValidHighlightLootableOptions
            = Enum.GetValues(typeof(HighlightLootableOptions)).Cast<HighlightLootableOptions>().Where(i => i != HighlightLootableOptions.Default);

        public static IEnumerable<FilterCategories> ValidFilterCategories
            = Enum.GetValues(typeof(FilterCategories)).Cast<FilterCategories>().Where(i => i != FilterCategories.Default);

        public static IEnumerable<SorterCategories> ValidSorterCategories
            = Enum.GetValues(typeof(SorterCategories)).Cast<SorterCategories>().Where(i => i != SorterCategories.Default);
    }

}
