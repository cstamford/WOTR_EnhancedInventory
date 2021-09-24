﻿using EnhancedInventory.Settings;
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

            if (Main.Settings.EnableSearchBar)
            {
                DrawSearchBarOptions();
                GUILayout.Space(12);
            }

            if (Main.Settings.EnableHighlightableLoot)
            {
                DrawHighlightableLootOptions();
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
            Main.Settings.EnableSearchBar = GUILayout.Toggle(Main.Settings.EnableSearchBar, " Enable search bar");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.EnableHighlightableLoot = GUILayout.Toggle(Main.Settings.EnableHighlightableLoot, " Highlight important items in the loot window");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.EnableVisualOverhaulSorting = GUILayout.Toggle(Main.Settings.EnableVisualOverhaulSorting, " Overhaul the visuals of the sort menu");
            GUILayout.EndHorizontal();
        }

        private static void DrawSearchBarOptions()
        {
            GUILayout.BeginHorizontal();
            bool draw_search_bar = FeatureButton("Search Bar");
            GUILayout.EndHorizontal();

            if (!draw_search_bar) return;

            GUILayout.BeginHorizontal();
            Main.Settings.SearchBarResetFilterWhenOpeningInv = GUILayout.Toggle(Main.Settings.SearchBarResetFilterWhenOpeningInv, " Reset the selected filter when opening the inventory screen");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.SearchBarFocusWhenOpeningInv = GUILayout.Toggle(Main.Settings.SearchBarFocusWhenOpeningInv, " Give the search bar focus when opening the inventory screen");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Main.Settings.SearchBarEnableCategoryButtons = GUILayout.Toggle(Main.Settings.SearchBarEnableCategoryButtons, " EXPERIMENTAL: Enable the old category buttons in addition to the search bar");
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            GUILayout.Space(8);
            bool draw_cats = FeatureButton("Enabled Filter Categories", false);
            GUILayout.EndHorizontal();

            if (!draw_cats) return;

            FilterCategories new_options = FilterCategories.NoFilter;

            foreach (FilterCategories flag in Enum.GetValues(typeof(FilterCategories)))
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

            HighlightLootableOptions new_options = HighlightLootableOptions.None;

            foreach (HighlightLootableOptions flag in Enum.GetValues(typeof(HighlightLootableOptions)))
            {
                if (flag == HighlightLootableOptions.None) continue;

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

            foreach (SorterCategories flag in Enum.GetValues(typeof(SorterCategories)))
            {
                if (flag == SorterCategories.NotSorted) continue;

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
