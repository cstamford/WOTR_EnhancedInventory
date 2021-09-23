using System;
using System.Collections.Generic;
using System.Linq;

namespace EnhancedInventory
{
    public static class ModInterop
    {
        private static readonly string[] m_legacy_mods = new string[]
        {
            "InventorySearchBar",
            "WeightValueSorting",
            "HighlightImportantLoot"
        };

        public static bool HasModConflicts()
        {
            IEnumerable<string> legacy_conflicts = AppDomain.CurrentDomain.GetAssemblies()
                .Where(i => m_legacy_mods.Any(j => i.FullName.Contains(j))).Select(i => i.FullName);
            
            if (legacy_conflicts.Any())
            {
                Main.Logger.Error("You have one or more legacy mods running. " +
                    "Enhanced Inventory contains their functionality, please disable them!:\n" +
                    legacy_conflicts.Aggregate((i, j) => i + "\n" + j));

                return true;
            }

            return false;
        }
    }
}
