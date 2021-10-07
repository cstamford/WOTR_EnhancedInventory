using EnhancedInventory.Settings;
using EnhancedInventory.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedInventory
{
    public static class SettingsGUI
    {
        private static readonly Dictionary<string, bool> m_visible_state = new Dictionary<string, bool>();

        public static void Draw()
        {
            DrawFeatureToggles();
            GUILayout.Space(12);

            if (Main.Settings.EnableInventorySearchBar)
            {
                DrawInventorySearchBarOptions();
                GUILayout.Space(12);
            }

            if (Main.Settings.EnableSpellbookSearchBar)
            {
                DrawSpellbookSearchBarOptions();
                GUILayout.Space(12);
            }

            if (Main.Settings.EnableHighlightableLoot)
            {
                DrawHighlightableLootOptions();
                GUILayout.Space(12);
            }

            if (Main.Settings.EnableCollectAllTweaks)
            {
                DrawCollectAllOptions();
                GUILayout.Space(12);
            }

            DrawSortingCategories();
        }

        private static void DrawFeatureToggles()
        {
            GUILayout.BeginHorizontal();
            bool draw_features = FeatureButton("Features");
            GUILayout.EndHorizontal();

            if (!draw_features) return;

            GUILayout.BeginHorizontal();
            Main.Settings.EnableInventorySearchBar = GUILayout.Toggle(Main.Settings.EnableInventorySearchBar, " Enables inventory functionality");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.EnableSpellbookSearchBar = GUILayout.Toggle(Main.Settings.EnableSpellbookSearchBar, " Enables spellbook functionality");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.EnableHighlightableLoot = GUILayout.Toggle(Main.Settings.EnableHighlightableLoot, " Highlight important items in the loot window");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.EnableVisualOverhaulSorting = GUILayout.Toggle(Main.Settings.EnableVisualOverhaulSorting, " Overhaul the visuals of the sort menu");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.EnableCollectAllTweaks = GUILayout.Toggle(Main.Settings.EnableCollectAllTweaks, " Tweak the behaviour of the Collect All button");
            GUILayout.EndHorizontal();
        }

        private static void DrawInventorySearchBarOptions()
        {
            GUILayout.BeginHorizontal();
            bool draw_search_bar = FeatureButton("Inventory");
            GUILayout.EndHorizontal();

            if (!draw_search_bar) return;

            GUILayout.BeginHorizontal();
            Main.Settings.InventorySearchBarResetFilterWhenOpening = GUILayout.Toggle(Main.Settings.InventorySearchBarResetFilterWhenOpening, " Reset the selected filter when opening the inventory screen");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.InventorySearchBarFocusWhenOpening = GUILayout.Toggle(Main.Settings.InventorySearchBarFocusWhenOpening, " Give the search bar focus when opening the inventory screen");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.InventorySearchBarScrollResetOnSubmit = GUILayout.Toggle(Main.Settings.InventorySearchBarScrollResetOnSubmit, " When pressing enter to complete a search, the scroll bar will reset to the top of the stash");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.InventorySearchBarEnableCategoryButtons = GUILayout.Toggle(Main.Settings.InventorySearchBarEnableCategoryButtons, " Enable hybrid mode; shows the old category buttons above the search bar");
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            GUILayout.Space(8);
            bool draw_criteria = FeatureButton("Inventory Search Criteria", false);
            GUILayout.EndHorizontal();

            if (draw_criteria)
            {
                InventorySearchCriteria new_options = default;

                foreach (InventorySearchCriteria flag in EnumHelper.ValidInventorySearchCriteria)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(12);

                    if (GUILayout.Toggle(Main.Settings.InventorySearchCriteria.HasFlag(flag), $" {flag}"))
                    {
                        new_options |= flag;
                    }

                    GUILayout.EndHorizontal();
                }

                Main.Settings.InventorySearchCriteria = new_options;

                GUILayout.Space(4);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(8);
            bool draw_cats = FeatureButton("Enabled Filter Categories", false);
            GUILayout.EndHorizontal();

            if (draw_cats)
            {
                FilterCategories new_options = FilterCategories.NoFilter;

                foreach (FilterCategories flag in EnumHelper.ValidFilterCategories)
                {
                    if (flag == FilterCategories.NoFilter) continue;

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(12);

                    if (GUILayout.Toggle(Main.Settings.FilterOptions.HasFlag(flag), $" {Main.FilterCategoryMap[flag].Item2 ?? flag.ToString()}"))
                    {
                        new_options |= flag;
                    }

                    GUILayout.EndHorizontal();
                }

                Main.Settings.FilterOptions = new_options;
            }
        }

        private static void DrawSpellbookSearchBarOptions()
        {
            GUILayout.BeginHorizontal();
            bool draw_search_bar = FeatureButton("Spellbook");
            GUILayout.EndHorizontal();

            if (!draw_search_bar) return;

            GUILayout.BeginHorizontal();
            Main.Settings.SpellbookSearchBarFocusWhenOpening = GUILayout.Toggle(Main.Settings.SpellbookSearchBarFocusWhenOpening, " Give the search bar focus when opening the spellbook screen");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.SpellbookShowAllSpellsByDefault = GUILayout.Toggle(Main.Settings.SpellbookShowAllSpellsByDefault, " Show all spell levels by default");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.SpellbookShowMetamagicByDefault = GUILayout.Toggle(Main.Settings.SpellbookShowMetamagicByDefault, " Show metamagic by default");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.SpellbookShowEmptyMetamagicCircles = GUILayout.Toggle(Main.Settings.SpellbookShowEmptyMetamagicCircles, " Show the empty grey metamagic circles above spells");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.SpellbookShowLevelWhenViewingAllSpells = GUILayout.Toggle(Main.Settings.SpellbookShowLevelWhenViewingAllSpells, " Show level of the spell when the spellbook is showing all spell levels");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.SpellbookAutoSwitchToMetamagicTab = GUILayout.Toggle(Main.Settings.SpellbookAutoSwitchToMetamagicTab, " After creating a metamagic spell, switch to the metamagic tab");
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            GUILayout.Space(8);
            bool draw_criteria = FeatureButton("Spellbook Search Criteria", false);
            GUILayout.EndHorizontal();

            if (draw_criteria)
            {
                SpellbookSearchCriteria new_options = default;

                foreach (SpellbookSearchCriteria flag in EnumHelper.ValidSpellbookSearchCriteria)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(12);

                    if (GUILayout.Toggle(Main.Settings.SpellbookSearchCriteria.HasFlag(flag), $" {flag}"))
                    {
                        new_options |= flag;
                    }

                    GUILayout.EndHorizontal();
                }

                Main.Settings.SpellbookSearchCriteria = new_options;

                GUILayout.Space(4);
            }
        }

        private static void DrawCollectAllOptions()
        {
            GUILayout.BeginHorizontal();
            bool draw_collect_all = FeatureButton("Collect All Tweaks");
            GUILayout.EndHorizontal();

            if (!draw_collect_all) return;

            GUILayout.BeginHorizontal();
            Main.Settings.CollectAllZeroWeightItems = GUILayout.Toggle(Main.Settings.CollectAllZeroWeightItems, " Collect all zero-weight items");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.CollectAllUnidentifiedItems = GUILayout.Toggle(Main.Settings.CollectAllUnidentifiedItems, " Collect all unidentified items");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.CollectAllUsefulItems = GUILayout.Toggle(Main.Settings.CollectAllUsefulItems, " Collect all useful (unlearned scrolls/recipes, unread books) items");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.CollectAllNotableItems = GUILayout.Toggle(Main.Settings.CollectAllNotableItems, " Collect all notable (golden border) items");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.CollectAllWeightValue = GUILayout.Toggle(Main.Settings.CollectAllWeightValue, " Use price / weight cutoff for loot (10 = masterwork weapon) ");
            string cutoff = GUILayout.TextField(Main.Settings.CollectAllWeightValueCutoff.ToString(), GUILayout.MinWidth(30));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            float cutoff_float = 0.0f;
            float.TryParse(cutoff, out cutoff_float);
            Main.Settings.CollectAllWeightValueCutoff = cutoff_float;
        }

        private static void DrawHighlightableLootOptions()
        {
            GUILayout.BeginHorizontal();
            bool draw_highlight = FeatureButton("Highlight Important Loot");
            GUILayout.EndHorizontal();

            if (!draw_highlight) return;

            GUILayout.BeginHorizontal();
            GUILayout.Space(8);

            GUILayout.BeginVertical();
            GUILayout.Label("Border Colour ");
            GUILayout.Label("Background Colour ");
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label(" Red");
            GUILayout.Label(" Red");
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            string border_r_str = GUILayout.TextField(Main.Settings.HighlightLootBorder.r.ToString(), GUILayout.MinWidth(30));
            string bg_r_str = GUILayout.TextField(Main.Settings.HighlightLootBackground.r.ToString(), GUILayout.MinWidth(30));
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label(" Green");
            GUILayout.Label(" Green");
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            string border_g_str = GUILayout.TextField(Main.Settings.HighlightLootBorder.g.ToString(), GUILayout.MinWidth(30));
            string bg_g_str = GUILayout.TextField(Main.Settings.HighlightLootBackground.g.ToString(), GUILayout.MinWidth(30));
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Label(" Blue");
            GUILayout.Label(" Blue");
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            string border_b_str = GUILayout.TextField(Main.Settings.HighlightLootBorder.b.ToString(), GUILayout.MinWidth(30));
            string bg_b_str = GUILayout.TextField(Main.Settings.HighlightLootBackground.b.ToString(), GUILayout.MinWidth(30));
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            uint.TryParse(border_r_str, out uint border_r);
            uint.TryParse(border_g_str, out uint border_g);
            uint.TryParse(border_b_str, out uint border_b);

            uint.TryParse(bg_r_str, out uint bg_r);
            uint.TryParse(bg_g_str, out uint bg_g);
            uint.TryParse(bg_b_str, out uint bg_b);

            Main.Settings.HighlightLootBorder = new Color32(
                (byte)Math.Max(0, Math.Min(255, border_r)),
                (byte)Math.Max(0, Math.Min(255, border_g)),
                (byte)Math.Max(0, Math.Min(255, border_b)),
                255);

            Main.Settings.HighlightLootBackground = new Color32(
                (byte)Math.Max(0, Math.Min(255, bg_r)),
                (byte)Math.Max(0, Math.Min(255, bg_g)),
                (byte)Math.Max(0, Math.Min(255, bg_b)),
                255);

            GUILayout.BeginHorizontal();
            GUILayout.Space(8);
            bool draw_highlight_cats = FeatureButton("Highlight Categories", false);
            GUILayout.EndHorizontal();

            if (!draw_highlight_cats) return;

            HighlightLootableOptions new_options = default;

            foreach (HighlightLootableOptions flag in EnumHelper.ValidHighlightLootableOptions)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(12);

                if (GUILayout.Toggle(Main.Settings.HighlightLootOptions.HasFlag(flag), $" {flag}"))
                {
                    new_options |= flag;
                }

                GUILayout.EndHorizontal();
            }

            Main.Settings.HighlightLootOptions = new_options;
        }

        private static void DrawSortingCategories()
        {
            GUILayout.BeginHorizontal();
            bool draw_cats = FeatureButton("Enabled Sorting Categories");
            GUILayout.EndHorizontal();

            if (!draw_cats) return;

            SorterCategories new_options = SorterCategories.NotSorted;

            foreach (SorterCategories flag in EnumHelper.ValidSorterCategories)
            {
                if (flag == SorterCategories.NotSorted || flag == SorterCategories.Default) continue;

                GUILayout.BeginHorizontal();

                if (GUILayout.Toggle(Main.Settings.SorterOptions.HasFlag(flag), $" {Main.SorterCategoryMap[flag].Item2 ?? flag.ToString()}"))
                {
                    new_options |= flag;
                }

                GUILayout.EndHorizontal();
            }

            Main.Settings.SorterOptions = new_options;
        }

        private static bool FeatureButton(string text, bool initial_state = true)
        {
            GUIStyle base_style = new GUIStyle(GUI.skin.GetStyle("Label"));
            base_style.fontStyle = FontStyle.Bold;
            base_style.normal.textColor = Color.white;

            if (!m_visible_state.ContainsKey(text))
            {
                m_visible_state[text] = initial_state;
            }

            bool prev_state = m_visible_state[text];
            string sign = prev_state ? "-" : "+";

            bool new_state = GUILayout.Button($"{sign} {text}", base_style) ? !prev_state : prev_state;
            m_visible_state[text] = new_state;

            return new_state;
        }
    }
}
