using EnhancedInventory.Settings;
using EnhancedInventory.Util;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Parts;
using Kingmaker.UI.Common;
using Kingmaker.UI.Group;
using Kingmaker.UI.ServiceWindow;
using System;
using System.Collections.Generic;

namespace EnhancedInventory.Hooks
{
    // Hook #1 out of #2 for filtering - this hook handled the custom filter categories.
    [HarmonyPatch(typeof(ItemsFilter), nameof(ItemsFilter.ShouldShowItem), new Type[] { typeof(ItemEntity), typeof(ItemsFilter.FilterType) })]
    public static class ItemsFilter_ShouldShowItem_ItemEntity
    {
        // Here, we handle filtering any expanded categories that we have.
        [HarmonyPrefix]
        public static bool Prefix(ItemEntity item, ItemsFilter.FilterType filter, ref bool __result)
        {
            ExpandedFilterType expanded_filter = (ExpandedFilterType)filter;

            if (expanded_filter == ExpandedFilterType.QuickslotUtilities)
            {
                __result = item.Blueprint is BlueprintItemEquipmentUsable blueprint &&
                    blueprint.Type != UsableItemType.Potion &&
                    blueprint.Type != UsableItemType.Scroll;
            }
            else if (expanded_filter == ExpandedFilterType.UnlearnedScrolls)
            {
                CopyScroll scroll = item.Blueprint.GetComponent<CopyScroll>();
                UnitEntityData unit = GroupController.Instance.GetCurrentCharacter();
                __result = scroll != null && unit != null && scroll.CanCopy(item, unit);
            }
            else if (expanded_filter == ExpandedFilterType.UnlearnedRecipes)
            {
                CopyRecipe recipe = item.Blueprint.GetComponent<CopyRecipe>();
                __result = recipe != null && recipe.CanCopy(item, null);
            }
            else if (expanded_filter == ExpandedFilterType.UnreadDocuments)
            {
                ItemPartShowInfoCallback cb = item.Get<ItemPartShowInfoCallback>();
                __result = cb != null && (!cb.m_Settings.Once || !cb.m_Triggered);
            }
            else if (expanded_filter == ExpandedFilterType.UsableWithoutUMD)
            {
                UnitEntityData unit = GroupController.Instance.GetCurrentCharacter();
                __result = item.Blueprint is BlueprintItemEquipmentUsable blueprint &&
                    (blueprint.Type == UsableItemType.Scroll || blueprint.Type == UsableItemType.Wand) &&
                    unit != null && !blueprint.IsUnitNeedUMDForUse(unit);
            }
            else if (expanded_filter == ExpandedFilterType.CurrentEquipped)
            {
                UnitEntityData unit = GroupController.Instance.GetCurrentCharacter();
                __result = unit != null;

                if (__result)
                {
                    bool weapon_match = item is ItemEntityWeapon weapon &&
                        ((unit.Body.PrimaryHand.HasWeapon && unit.Body.PrimaryHand.Weapon.Blueprint.Type == weapon.Blueprint.Type) ||
                        (unit.Body.SecondaryHand.HasWeapon && unit.Body.SecondaryHand.Weapon.Blueprint.Type == weapon.Blueprint.Type));

                    bool shield_match = item is ItemEntityShield shield &&
                        ((unit.Body.PrimaryHand.HasShield && unit.Body.PrimaryHand.Shield.Blueprint.Type == shield.Blueprint.Type) ||
                        (unit.Body.SecondaryHand.HasShield && unit.Body.SecondaryHand.Shield.Blueprint.Type == shield.Blueprint.Type));

                    bool armour_match = item is ItemEntityArmor armor &&
                        unit.Body.Armor.HasArmor && unit.Body.Armor.Armor.Blueprint.ProficiencyGroup == armor.Blueprint.ProficiencyGroup;

                    __result = weapon_match || shield_match || armour_match;
                }
            }
            else if (expanded_filter == ExpandedFilterType.NonZeroPW)
            {
                __result = item.Blueprint.SellPrice > 0 && item.Blueprint.Weight > 0.0f;
            }
            else
            {
                // Original call - proceed as normal.
                return true;
            }

            // This call to the blueprint version will skip original in prefix then apply the search bar logic in postfix.
            __result = __result && ItemsFilter.ShouldShowItem(item.Blueprint, filter);
            return false;
        }
    }

    // Hook #2 out of #2 for filtering - this hook handles filtering based on string search.
    [HarmonyPatch(typeof(ItemsFilter), nameof(ItemsFilter.ShouldShowItem), new Type[] { typeof(BlueprintItem), typeof(ItemsFilter.FilterType) })]
    public static class ItemsFilter_ShouldShowItem_Blueprint
    {
        public static string SearchContents = null;

        // Prefix: If we're filtering one of the expanded categories, we require more than the blueprint - we require the instance.
        // If someone calls the function to check the blueprint directly, for expanded categories, we must simply allow everything.
        [HarmonyPrefix]
        public static bool Prefix(ItemsFilter.FilterType filter, ref bool __result)
        {
            __result = true;
            return (int)filter < (int)ExpandedFilterType.QuickslotUtilities;
        }

        // Postfix: We apply the string match, if any, to the resulting matches from the original call (or our prefix).
        [HarmonyPostfix]
        public static void Postfix(BlueprintItem blueprintItem, ref bool __result)
        {
            if (__result && !string.IsNullOrWhiteSpace(SearchContents))
            {
                __result = false;

                if (Main.Settings.InventorySearchCriteria.HasFlag(InventorySearchCriteria.ItemName))
                {
                    __result |= blueprintItem.Name.IndexOf(SearchContents, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                if (Main.Settings.InventorySearchCriteria.HasFlag(InventorySearchCriteria.ItemType))
                {
                    __result |= blueprintItem.ItemType.ToString().IndexOf(SearchContents, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                if (Main.Settings.InventorySearchCriteria.HasFlag(InventorySearchCriteria.ItemSubtype))
                {
                    __result |= blueprintItem.SubtypeName.IndexOf(SearchContents, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                if (Main.Settings.InventorySearchCriteria.HasFlag(InventorySearchCriteria.ItemDescription))
                {
                    __result |= blueprintItem.Description.IndexOf(SearchContents, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
        }
    }

    // Sorting - this hook handles custom sorting categories.
    [HarmonyPatch(typeof(ItemsFilter))]
    public static class ItemsFilter_ItemSorter
    {
        private static int CompareByWeightValue(ItemEntity a, ItemEntity b, ItemsFilter.FilterType filter)
        {
            float a_weight_value = a.Blueprint.Weight <= 0.0f ? float.PositiveInfinity : a.Blueprint.Cost / a.Blueprint.Weight;
            float b_weight_value = b.Blueprint.Weight <= 0.0f ? float.PositiveInfinity : b.Blueprint.Cost / b.Blueprint.Weight;
            return a_weight_value == b_weight_value ? ItemsFilter.CompareByTypeAndName(a, b, filter) : (a_weight_value > b_weight_value ? 1 : -1);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ItemsFilter.CompareByTypeAndName))]
        private static bool CompareByTypeAndName(ItemEntity a, ItemEntity b, ItemsFilter.FilterType filter, ref int __result)
        {
            // First by main type
            int a_b_comparison = a.Blueprint.ItemType.CompareTo(b.Blueprint.ItemType);
            if (a_b_comparison != 0)
            {
                __result = a_b_comparison;
                return false;
            }

            // Then by subtype
            a_b_comparison = string.Compare(a.Blueprint.SubtypeName, b.Blueprint.SubtypeName, StringComparison.OrdinalIgnoreCase);
            if (a_b_comparison != 0)
            {
                __result = a_b_comparison;
                return false;
            }

            // Finally by name
            __result = string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ItemsFilter.ItemSorter))]
        public static bool Prefix(ItemsFilter.SorterType type, List<ItemEntity> items, ItemsFilter.FilterType filter, ref List<ItemEntity> __result)
        {
            ExpandedSorterType expanded_type = (ExpandedSorterType)type;

            if (expanded_type == ExpandedSorterType.WeightValueUp)
            {
                items.Sort((ItemEntity a, ItemEntity b) => CompareByWeightValue(a, b, filter));
            }
            else if (expanded_type == ExpandedSorterType.WeightValueDown)
            {
                items.Sort((ItemEntity a, ItemEntity b) => CompareByWeightValue(a, b, filter));
                items.Reverse();       
            }
            else
            {
                return true;
            }

            __result = items;
            return false;
        }
    }
}