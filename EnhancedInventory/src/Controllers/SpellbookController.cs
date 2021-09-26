﻿using EnhancedInventory.Util;
using Kingmaker.UI.MVVM._PCView.ServiceWindows.Spellbook;
using Kingmaker.UI.MVVM._PCView.ServiceWindows.Spellbook.KnownSpells;
using Owlcat.Runtime.UniRx;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._VM.ServiceWindows.Spellbook.KnownSpells;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Controls.Button;
using Kingmaker.UI.MVVM._VM.ServiceWindows.Spellbook.Metamagic;
using HarmonyLib;
using Kingmaker.UI;
using TMPro;
using Kingmaker.UI.MVVM._PCView.ServiceWindows.Spellbook.Switchers;
using UnityEngine.UI;
using Kingmaker.UI.MVVM._VM.ServiceWindows.Spellbook.Switchers;

namespace EnhancedInventory.Controllers
{
    public class DummyKnownSpellsView : SpellbookKnownSpellsPCView
    {
        public override void BindViewImplementation() { }
        public override void DestroyViewImplementation() { }
    }

    public class SpellbookController : MonoBehaviour
    {
        private SearchBar m_search_bar;
        private SpellbookKnownSpellPCView m_known_spell_prefab;
        private IReactiveProperty<Spellbook> m_spellbook;
        private IReactiveProperty<SpellbookLevelVM> m_spellbook_level;
        private IReactiveProperty<AbilityDataVM> m_selected_spell;

        private ToggleWorkaround m_metamagic_checkbox;
        private Toggle m_all_button;

        private List<IDisposable> m_handlers = new List<IDisposable>();
        private bool m_deferred_update = true;

        private void Awake()
        {
            m_search_bar = new SearchBar(transform.Find("MainContainer"), "Enter spell name...");

            m_search_bar.GameObject.transform.localScale = new Vector3(0.85f, 0.85f, 1.0f);
            m_search_bar.GameObject.transform.localPosition = new Vector2(-63.0f, 390.0f);

            m_search_bar.Dropdown.onValueChanged.AddListener(delegate (int val)
            {
                m_deferred_update = true;
                transform.Find("MainContainer/KnownSpells/StandardScrollView").GetComponent<ScrollRectExtended>().ScrollToTop();
            });

            m_search_bar.InputField.onValueChanged.AddListener(delegate (string _) { m_deferred_update = true; });

            // Setup string options...
            List<string> options = Enum.GetValues(typeof(SpellbookFilter)).Cast<SpellbookFilter>().Select(i => i.ToString()).ToList();
            options[(int)SpellbookFilter.NoFilter] = "No filter";
            options[(int)SpellbookFilter.TargetsFortitude] = "Spell targets fortitude";
            options[(int)SpellbookFilter.TargetsReflex] = "Spell targets reflex";
            options[(int)SpellbookFilter.TargetsWill] = "Spell targets will";
            m_search_bar.Dropdown.AddOptions(options);

            // Make a dummy view that does nothing - we handle the logic in here.
            GetComponentInParent<SpellbookPCView>().m_KnownSpellsView = new DummyKnownSpellsView();

            // Grab what we need from the old view then destroy it.
            SpellbookKnownSpellsPCView old_view = GetComponentInChildren<SpellbookKnownSpellsPCView>();
            m_known_spell_prefab = old_view.m_KnownSpellView;
            Destroy(old_view);

            // Disable the current spell level indicator, it isn't used any more.
            Destroy(transform.Find("MainContainer/Information/CurrentLevel").gameObject);

            // Create button to toggle metamagic.
            GameObject metamagic_button = Instantiate(transform.Find("MainContainer/KnownSpells/Toggle").gameObject, transform.Find("MainContainer/KnownSpells"));
            metamagic_button.name = "ToggleMetamagic";
            metamagic_button.transform.localPosition = new Vector2(501.0f, -405.0f);
            metamagic_button.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = "Show metamagic";
            m_metamagic_checkbox = metamagic_button.GetComponent<ToggleWorkaround>();
            m_metamagic_checkbox.onValueChanged.AddListener(delegate (bool _) { m_deferred_update = true; });
            m_metamagic_checkbox.isOn = true;

            // Move the levels display (which is still used for displaying memorized spells).
            // Also hide the metamagic option.
            Transform levels = transform.Find("MainContainer/Levels");
            levels.localPosition = new Vector2(506.0f, 382.0f);

            // TODO: Bring this back later...
            Destroy(transform.Find("MainContainer/KnownSpells/Toggle").gameObject);
        }

        private void OnDisable()
        {
            Destroy(m_all_button.gameObject);
            m_spellbook = null;
        }

        private void Update()
        {
            m_deferred_update |= m_spellbook == null;

            if (m_spellbook == null)
            {
                // Add button for "all spell levels".
                Transform levels = transform.Find("MainContainer/Levels");

                GameObject all = Instantiate(levels.GetChild(0).gameObject, levels.parent);
                all.name = "AllButton";
                all.transform.localPosition = new Vector2(191.0f, 382.0f);

                TextMeshProUGUI all_text = all.transform.Find("LevelLabel").GetComponent<TextMeshProUGUI>();
                all_text.text = "All";
                all_text.fontSize = 26;

                all.transform.Find("Image").GetComponent<Image>().overrideSprite = null;

                m_all_button = all.AddComponent<Toggle>();

                m_all_button.onValueChanged.AddListener(delegate (bool state)
                {
                    m_deferred_update = true;
                    all.transform.Find("Active").gameObject.SetActive(state);
                });

                m_all_button.isOn = true;

                Destroy(all.GetComponent<OwlcatMultiButton>());
                Destroy(all.GetComponent<SpellbookLevelSwitcherEntityPCView>());

                // Grab the various state we need...
                SpellbookPCView spellbook_pc_view = GetComponentInParent<SpellbookPCView>();
                m_spellbook = spellbook_pc_view.ViewModel.CurrentSpellbook;
                m_spellbook_level = spellbook_pc_view.ViewModel.CurrentSpellbookLevel;
                m_selected_spell = spellbook_pc_view.ViewModel.CurrentSelectedSpell;

                m_spellbook_level.Subscribe(delegate (SpellbookLevelVM _) { m_deferred_update = true; });

                // This event is fired when the metamagic builder is opened or shut.
                spellbook_pc_view.ViewModel.MetamagicBuilderMode.Subscribe(delegate (bool state)
                {
                    if (!state) return;

                    // If we've been opened, we need to register for the callback every time a new spell is created.
                    Action old_action = spellbook_pc_view.ViewModel.SpellbookMetamagicMixerVM.m_OnComplete;
                    AccessTools.FieldRef<SpellbookMetamagicMixerVM, Action> field = AccessTools.FieldRefAccess<SpellbookMetamagicMixerVM, Action>(nameof(SpellbookMetamagicMixerVM.m_OnComplete));
                    field.Invoke(spellbook_pc_view.ViewModel.SpellbookMetamagicMixerVM) = delegate
                    {
                        m_deferred_update = true;
                        old_action();
                    };
                });

                // This event is fired when changing spellbook or updating the spells inside the spellbook.
                spellbook_pc_view.m_CharacteristicsView.ViewModel.RefreshCommand.ObserveLastValueOnLateUpdate().Subscribe(delegate (Unit _)
                {
                    m_deferred_update = true;
                });

                if (Main.Settings.SpellbookSearchBarFocusWhenOpening)
                {
                    m_search_bar.FocusSearchBar();
                }
            }

            if (m_deferred_update)
            {
                foreach (IDisposable handler in m_handlers)
                {
                    handler.Dispose();
                }

                m_handlers.Clear();

                WidgetListMVVM widgets = transform.Find("MainContainer/KnownSpells").GetComponent<WidgetListMVVM>();
                widgets.Clear();

                if (m_spellbook.Value != null)
                {
                    SpellbookFilter filter = (SpellbookFilter)m_search_bar.Dropdown.value;
                    List<AbilityDataVM> spells_as_widget = new List<AbilityDataVM>();

                    int spellbook_level = m_spellbook_level.Value.Level;

                    for (int level = 0; level <= 9; ++level)
                    {
                        if (!m_all_button.isOn && spellbook_level != 10 && level != spellbook_level) continue;

                        foreach (AbilityData spell in UIUtilityUnit.GetKnownSpellsForLevel(level, m_spellbook.Value))
                        {
                            if (!m_metamagic_checkbox.isOn && spell.MetamagicData != null) continue;
                            if (spellbook_level == 10 && spell.MetamagicData == null) continue;

                            string save = spell.Blueprint.LocalizedSavingThrow;

                            if (filter == SpellbookFilter.TargetsFortitude ||
                                filter == SpellbookFilter.TargetsReflex ||
                                filter == SpellbookFilter.TargetsWill)
                            {
                                if (string.IsNullOrWhiteSpace(save)) continue;
                                else if (filter == SpellbookFilter.TargetsFortitude && save.IndexOf("Fortitude", StringComparison.OrdinalIgnoreCase) == -1) continue;
                                else if (filter == SpellbookFilter.TargetsReflex && save.IndexOf("Reflex", StringComparison.OrdinalIgnoreCase) == -1) continue;
                                else if (filter == SpellbookFilter.TargetsWill && save.IndexOf("Will", StringComparison.OrdinalIgnoreCase) == -1) continue;
                            }

                            bool proceed = false;

                            string text = m_search_bar.InputField.text;

                            if (string.IsNullOrWhiteSpace(text))
                            {
                                proceed = true;
                            }
                            else if (Main.Settings.SpellbookSearchCriteria.HasFlag(Settings.SpellbookSearchCriteria.SpellName) &&
                                spell.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                proceed = true;
                            }
                            else if (Main.Settings.SpellbookSearchCriteria.HasFlag(Settings.SpellbookSearchCriteria.SpellDescription) &&
                                spell.Description.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                proceed = true;
                            }
                            else if (Main.Settings.SpellbookSearchCriteria.HasFlag(Settings.SpellbookSearchCriteria.SpellSaves) &&
                                save.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                proceed = true;
                            }
                            else if (Main.Settings.SpellbookSearchCriteria.HasFlag(Settings.SpellbookSearchCriteria.SpellSchool) &&
                                spell.Blueprint.School.ToString().IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                proceed = true;
                            }

                            if (proceed)
                            {
                                spells_as_widget.Add(new AbilityDataVM(spell, m_spellbook.Value, m_selected_spell));
                            }
                        }
                    }

                    widgets.DrawEntries(spells_as_widget
                        .OrderBy(i => i.SpellLevel).ThenBy(i => i.DisplayName).ToArray(),
                        new List<IWidgetView> { m_known_spell_prefab });

                    foreach (SpellbookKnownSpellPCView spell in transform
                        .Find("MainContainer/KnownSpells/StandardScrollView/Viewport/Content")
                        .GetComponentsInChildren<SpellbookKnownSpellPCView>())
                    {
                        // Event per slot in the prefab to change the selected option.
                        m_handlers.Add(spell.m_Button.OnLeftClickAsObservable().Subscribe(delegate (Unit _)
                        {
                            SelectMemorisationLevel(spell.ViewModel.SpellLevel);
                        }));
                    }

                    m_deferred_update = false;
                }
            }
        }

        private void SelectMemorisationLevel(int level)
        {
            Transform levels = transform.Find("MainContainer/Levels");
            levels.GetChild(level).GetComponent<OwlcatMultiButton>().OnLeftClick.Invoke();
        }
    }
}