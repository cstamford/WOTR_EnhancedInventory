
using EnhancedInventory.Settings;
using EnhancedInventory.Util;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._PCView.CharGen.Phases.FeatureSelector;
using Kingmaker.UI.MVVM._PCView.Loot;
using Kingmaker.UI.MVVM._PCView.ServiceWindows.Inventory;
using Kingmaker.UI.MVVM._PCView.Vendor;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.UI.Controls.Button;
using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EnhancedInventory.Controllers
{
    public class SearchBarController : MonoBehaviour
    {
        private TMP_InputField m_input_field;
        private OwlcatButton m_input_button;
        private OwlcatButton m_dropdown_button;
        private GameObject m_dropdown_icon;
        private TMP_Dropdown m_dropdown;
        private TextMeshProUGUI m_placeholder;
        private Image[] m_search_icons;
        private ReactiveProperty<ItemsFilter.FilterType> m_active_filter;
        private IDisposable m_char_selection_changed_cb;

        private void Awake()
        {
            // -- Find all of our fields and setup handlers

            m_input_field = transform.Find("FieldPlace/SearchField/SearchBackImage/InputField").GetComponent<TMP_InputField>();
            m_input_button = transform.Find("FieldPlace/SearchField/SearchBackImage/Placeholder").GetComponent<OwlcatButton>();
            m_dropdown_button = transform.Find("FieldPlace/SearchField/SearchBackImage/Dropdown/GenerateButtonPlace").GetComponent<OwlcatButton>();
            m_dropdown_icon = transform.Find("FieldPlace/SearchField/SearchBackImage/Dropdown/GenerateButtonPlace/GenerateButton/Icon").gameObject;
            m_dropdown = transform.Find("FieldPlace/SearchField/SearchBackImage/Dropdown").GetComponent<TMP_Dropdown>();
            m_placeholder = transform.Find("FieldPlace/SearchField/SearchBackImage/Placeholder/Label").GetComponent<TextMeshProUGUI>();
            m_input_field.transform.Find("Text Area/Placeholder").GetComponent<TextMeshProUGUI>().SetText("Enter item name...");

            m_input_field.onValueChanged.AddListener(delegate (string _) { OnEdit(); });
            m_input_field.onEndEdit.AddListener(delegate (string _) { OnEndEdit(); });
            m_input_button.OnLeftClick.AddListener(delegate { OnStartEdit(); });
            m_dropdown_button.OnLeftClick.AddListener(delegate { OnShowDropdown(); });
            m_dropdown.onValueChanged.AddListener(delegate (int _) { OnSelectDropdown(); });

            m_char_selection_changed_cb = Game.Instance.SelectionCharacter.SelectedUnit.Subscribe(delegate (UnitDescriptor _) { ApplyFilter(); });

            Destroy(GetComponent<CharGenFeatureSearchPCView>()); // controller from where we stole the search bar

            // -- Dropdown options

            m_dropdown.ClearOptions();

            List<string> options = new List<string>();

            foreach (FilterCategories flag in Enum.GetValues(typeof(FilterCategories)))
            {
                if (Main.Settings.FilterOptions.HasFlag(flag))
                {
                    (int idx, string text) = Main.FilterCategoryMap[flag];

                    if (text == null)
                    {
                        ItemsFilter.FilterType localization_enum = (ItemsFilter.FilterType)idx;

                        // For whatever reason, the localization DB has the wrong info for some of these options... I suspect someone changed the enum order
                        // around and these particular strings are not used anywhere.

                        switch (idx)
                        {
                            case (int)ItemsFilter.FilterType.Ingredients:    localization_enum = ItemsFilter.FilterType.NonUsable; break;
                            case (int)ItemsFilter.FilterType.Usable:         localization_enum = ItemsFilter.FilterType.Ingredients; break;
                            case (int)ItemsFilter.FilterType.NonUsable:      localization_enum = ItemsFilter.FilterType.Usable; break;
                        }

                        text = LocalizedTexts.Instance.ItemsFilter.GetText(localization_enum);
                        Main.FilterCategoryMap[flag] = new Tuple<int, string>(idx, text);
                    }

                    options.Add(text);
                }
            }

            m_dropdown.AddOptions(options);

            // -- Dropdown images

            List<Image> images = new List<Image>();
            GameObject switch_bar = transform.parent.Find("SwitchBar").gameObject;

            foreach (Transform child in switch_bar.transform)
            {
                images.Add(child.Find("Icon")?.GetComponent<Image>());
            }

            while (images.Count < options.Count)
            {
                images.Add(null);
            }

            m_search_icons = images.ToArray();

            // -- Positioning

            RectTransform our_transform = GetComponent<RectTransform>();

            if (Main.Settings.SearchBarEnableCategoryButtons)
            {
                our_transform.localScale = new Vector3(0.6f, 0.6f, 1.0f);
                our_transform.localPosition = new Vector3(0.0f, -8.0f, 0.0f);

                RectTransform their_transform = switch_bar.GetComponent<RectTransform>();
                their_transform.localPosition = new Vector3(
                    their_transform.localPosition.x,
                    their_transform.localPosition.y + 23.0f,
                    their_transform.localPosition.z);
                their_transform.localScale = new Vector3(0.6f, 0.6f, 1.0f);

                // destroy the top and bottom gfx as they cause a lot of noise
                Destroy(transform.Find("Background/Decoration/TopLineImage").gameObject);
                Destroy(transform.Find("Background/Decoration/BottomLineImage").gameObject);
            }
            else
            {
                our_transform.localScale = new Vector3(0.85f, 0.85f, 1.0f);
                our_transform.localPosition = new Vector3(0.0f, 2.0f, 0.0f);
                Destroy(switch_bar);
            }

            // -- Style: search bar dropdown

            Destroy(m_dropdown.template.Find("Viewport/TopBorderImage").gameObject);
            Transform border = m_dropdown.template.Find("Viewport/Content/Item/BottomBorderImage");
            RectTransform rect = border.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.0f, 0.0f);
            rect.anchorMax = new Vector2(1.0f, 0.0f);
            rect.offsetMin = new Vector2(0.0f, 0.0f);
            rect.offsetMax = new Vector2(0.0f, 2.0f);
        }

        private void OnDestroy()
        {
            m_char_selection_changed_cb.Dispose();
        }

        private void ApplyFilter()
        {
            if (m_active_filter != null)
            {
                Hooks.ItemsFilter_ShouldShowItem_Blueprint.SearchContents = m_input_field.text;
                m_active_filter.SetValueAndForceNotify((ItemsFilter.FilterType)Main.FilterMapper.To(m_dropdown.value));
                Hooks.ItemsFilter_ShouldShowItem_Blueprint.SearchContents = null;
            }
        }

        private void OnEnable()
        {
            m_active_filter = null;
        }

        private void Update()
        {
            if (m_active_filter == null)
            {
                string path = transform.GetPath();

                if (path.Contains("LootPCView/Window/Inventory")) // pc side - stash/loot
                {
                    LootInventoryStashPCView inventory_pc_view = GetComponentInParent<LootInventoryStashPCView>();
                    m_active_filter = inventory_pc_view.ViewModel.ItemsFilter.CurrentFilter;
                    inventory_pc_view.ViewModel.ItemSlotsGroup.CollectionChangedCommand.Subscribe(delegate (bool _) { ApplyFilter(); });
                    inventory_pc_view.ViewModel.ItemsFilter.CurrentSorter.Subscribe(delegate (ItemsFilter.SorterType _) { ApplyFilter(); });
                }
                else if (path.Contains("LootPCView/Window/Collector")) // loot side - stash/loot
                {
                    LootCollectorPCView collector_pc_view = GetComponentInParent<LootCollectorPCView>();
                    m_active_filter = collector_pc_view.ViewModel.ItemsFilter.CurrentFilter;
                    collector_pc_view.ViewModel.CollectionChangedCommand.Subscribe(delegate (Unit _) { ApplyFilter(); });
                    collector_pc_view.ViewModel.ItemsFilter.CurrentSorter.Subscribe(delegate (ItemsFilter.SorterType _) { ApplyFilter(); });
                }
                else if (path.Contains("VendorPCView/MainContent/VendorBlock")) // vendor - vendor side
                {
                    VendorPCView vendor_pc_view = GetComponentInParent<VendorPCView>();
                    m_active_filter = vendor_pc_view.ViewModel.VendorItemsFilter.CurrentFilter;
                    vendor_pc_view.ViewModel.VendorSlotsGroup.CollectionChangedCommand.Subscribe(delegate (bool _) { ApplyFilter(); });
                    vendor_pc_view.ViewModel.VendorItemsFilter.CurrentSorter.Subscribe(delegate (ItemsFilter.SorterType _) { ApplyFilter(); });
                }
                else
                {
                    InventoryStashPCView stash_pc_view = GetComponentInParent<InventoryStashPCView>();
                    m_active_filter = stash_pc_view.ViewModel.ItemsFilter.CurrentFilter;
                    stash_pc_view.ViewModel.ItemSlotsGroup.CollectionChangedCommand.Subscribe(delegate (bool _) { ApplyFilter(); });
                    stash_pc_view.ViewModel.ItemsFilter.CurrentSorter.Subscribe(delegate (ItemsFilter.SorterType _) { ApplyFilter(); });
                }

                Transform switch_bar = transform.parent.Find("SwitchBar");

                if (switch_bar != null)
                {
                    // Add listeners to each button; if the button changes, we change the dropdown to match.
                    foreach (ItemsFilter.FilterType filter in Enum.GetValues(typeof(ItemsFilter.FilterType)))
                    {
                        int idx = (int)filter;
                        int mapped_idx = Main.FilterMapper.From(idx);

                        if (mapped_idx == -1)
                        {
                            switch_bar.transform.GetChild(idx).gameObject.SetActive(false);
                        }
                        else
                        {
                            ToggleWorkaround toggle = switch_bar.transform.GetChild(idx).GetComponent<ToggleWorkaround>();
                            toggle.onValueChanged.AddListener(delegate (bool on) { if (on) m_dropdown.value = mapped_idx; });
                        }
                    }
                }

                if (Main.Settings.SearchBarResetFilterWhenOpeningInv)
                {
                    m_dropdown.value = Main.FilterMapper.From((int)ItemsFilter.FilterType.NoFilter);
                }
            }
        }

        private void OnShowDropdown()
        {
            m_dropdown.Show();
        }

        private void OnSelectDropdown()
        {
            UpdatePlaceholder();
            ApplyFilter();
        }

        private void OnStartEdit()
        {
            m_input_button.gameObject.SetActive(false);
            m_input_field.gameObject.SetActive(true);
            m_input_field.Select();
            m_input_field.ActivateInputField();
        }

        private void OnEdit()
        {
            UpdatePlaceholder();
            ApplyFilter();
        }

        private void OnEndEdit()
        {
            m_input_field.gameObject.SetActive(false);
            m_input_button.gameObject.SetActive(true);

            if (!EventSystem.current.alreadySelecting) // could be, in same click, ending edit and starting dropdown
            {
                EventSystem.current.SetSelectedGameObject(gameObject); // return focus to regular UI
            }
        }

        private void UpdatePlaceholder()
        {
            m_dropdown_icon.GetComponent<Image>().sprite = m_search_icons[m_dropdown.value]?.sprite;
            m_dropdown_icon.gameObject.SetActive(m_dropdown_icon.GetComponent<Image>().sprite != null);
            m_placeholder.text = string.IsNullOrEmpty(m_input_field.text) ? m_dropdown.options[m_dropdown.value].text : m_input_field.text;
        }
    }
}
