using EnhancedInventory.Util;
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
        private IReactiveProperty<AbilityDataVM> m_selected_spell;
        private List<IDisposable> m_handlers = new List<IDisposable>();

        private bool m_deferred_update = true;

        private void Awake()
        {
            m_search_bar = new SearchBar(transform.Find("MainContainer"), "Enter spell name...");

            m_search_bar.GameObject.transform.localScale = new Vector3(0.85f, 0.85f, 1.0f);
            m_search_bar.GameObject.transform.localPosition = new Vector2(325.0f, 390.0f);

            m_search_bar.Dropdown.onValueChanged.AddListener(delegate (int val)
            {
                m_deferred_update = true;

                // Reset scroll.
                transform.Find("MainContainer/KnownSpells/StandardScrollView").GetComponent<ScrollRectExtended>().ScrollToTop();

                // If choosing a spell level, also select the appropriate memorisation option.
                if (val >= (int)SpellbookFilter.SpellLevel0 && val <= (int)SpellbookFilter.SpellLevel9)
                {
                    SelectMemorisationLevel(val - (int)SpellbookFilter.SpellLevel0);
                }
            });

            m_search_bar.InputField.onValueChanged.AddListener(delegate (string _) { m_deferred_update = true; });

            // Setup string options...
            List<string> options = Enum.GetValues(typeof(SpellbookFilter)).Cast<SpellbookFilter>().Select(i => i.ToString()).ToList();
            options[(int)SpellbookFilter.NoFilter] = "No filter";
            options[(int)SpellbookFilter.TargetsFortitude] = "Spell targets fortitude";
            options[(int)SpellbookFilter.TargetsReflex] = "Spell targets reflex";
            options[(int)SpellbookFilter.TargetsWill] = "Spell targets will";
            options[(int)SpellbookFilter.SpellLevel0] = "Cantrips";
            options[(int)SpellbookFilter.SpellLevel1] = "1st level";
            options[(int)SpellbookFilter.SpellLevel2] = "2nd level";
            options[(int)SpellbookFilter.SpellLevel3] = "3rd level";
            options[(int)SpellbookFilter.SpellLevel4] = "4th level";
            options[(int)SpellbookFilter.SpellLevel5] = "5th level";
            options[(int)SpellbookFilter.SpellLevel6] = "6th level";
            options[(int)SpellbookFilter.SpellLevel7] = "7th level";
            options[(int)SpellbookFilter.SpellLevel8] = "8th level";
            options[(int)SpellbookFilter.SpellLevel9] = "9th level";
            m_search_bar.Dropdown.AddOptions(options);

            // Make a dummy view that does nothing - we handle the logic in here.
            GetComponentInParent<SpellbookPCView>().m_KnownSpellsView = new DummyKnownSpellsView();

            // Grab what we need from the old view then destroy it.
            SpellbookKnownSpellsPCView old_view = GetComponentInChildren<SpellbookKnownSpellsPCView>();
            m_known_spell_prefab = old_view.m_KnownSpellView;
            Destroy(old_view);

            // Disable the current spell level indicator, it isn't used any more.
            Destroy(transform.Find("MainContainer/Information/CurrentLevel").gameObject);

            // TEMP - fix later...
            Destroy(transform.Find("MainContainer/KnownSpells/Toggle").gameObject);
        }

        private void OnEnable()
        {
            m_spellbook = null;
        }

        private void Update()
        {
            m_deferred_update |= m_spellbook == null;

            if (m_spellbook == null)
            {
                // Move the levels display (which is still used for displaying memorized spells).
                // Also hide the metamagic option.
                Transform levels = transform.Find("MainContainer/Levels");
                levels.localPosition = new Vector2(-450.0f, -73.0f);
                levels.localScale = new Vector3(0.6f, 0.6f, 1.0f);

                // Grab the various state we need...
                SpellbookPCView spellbook_pc_view = GetComponentInParent<SpellbookPCView>();
                m_spellbook = spellbook_pc_view.ViewModel.CurrentSpellbook;
                m_selected_spell = spellbook_pc_view.ViewModel.CurrentSelectedSpell;
                m_spellbook.Subscribe(delegate (Spellbook book) { m_deferred_update = true; });

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
                    List<AbilityDataVM> spells_as_widget = new List<AbilityDataVM>();

                    int filter = m_search_bar.Dropdown.value;
                    SpellbookFilter filter_enum = (SpellbookFilter)filter;
                    bool filter_on_spell_level = filter >= (int)SpellbookFilter.SpellLevel0 && filter <= (int)SpellbookFilter.SpellLevel9;

                    for (int level = 0; level <= 9; ++level)
                    {
                        if (filter_on_spell_level && level != filter - (int)SpellbookFilter.SpellLevel0) continue;

                        foreach (AbilityData spell in UIUtilityUnit.GetKnownSpellsForLevel(level, m_spellbook.Value))
                        {
                            string save = spell.Blueprint.LocalizedSavingThrow;

                            if (filter_enum == SpellbookFilter.TargetsFortitude ||
                                filter_enum == SpellbookFilter.TargetsReflex ||
                                filter_enum == SpellbookFilter.TargetsWill)
                            {
                                if (string.IsNullOrWhiteSpace(save)) continue;
                                else if (filter_enum == SpellbookFilter.TargetsFortitude && save.IndexOf("Fortitude", StringComparison.OrdinalIgnoreCase) == -1) continue;
                                else if (filter_enum == SpellbookFilter.TargetsReflex && save.IndexOf("Reflex", StringComparison.OrdinalIgnoreCase) == -1) continue;
                                else if (filter_enum == SpellbookFilter.TargetsWill && save.IndexOf("Will", StringComparison.OrdinalIgnoreCase) == -1) continue;
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
