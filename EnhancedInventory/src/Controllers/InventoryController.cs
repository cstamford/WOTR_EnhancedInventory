
using EnhancedInventory.Settings;
using EnhancedInventory.Util;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._PCView.Loot;
using Kingmaker.UI.MVVM._PCView.ServiceWindows.Inventory;
using Kingmaker.UI.MVVM._PCView.Vendor;
using Kingmaker.UnitLogic;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedInventory.Controllers
{
    public enum InventoryType
    {
        InventoryStash,
        Vendor,
        LootCollector,
        LootInventoryStash
    }

    public class InventoryController : MonoBehaviour
    {
        public InventoryType Type;

        private Transform m_filter_block;
        private SearchBar m_search_bar;
        private Image[] m_search_icons;
        private ReactiveProperty<ItemsFilter.FilterType> m_active_filter;
        private IDisposable m_char_selection_changed_cb;

        private void Awake()
        {
            m_filter_block = transform.Find(PathToFilterBlock());
            m_search_bar = new SearchBar(m_filter_block);

            m_search_bar.Dropdown.onValueChanged.AddListener(delegate (int _)
            {
                m_search_bar.DropdownIconObject.GetComponent<Image>().sprite = m_search_icons[m_search_bar.Dropdown.value]?.sprite;
                m_search_bar.DropdownIconObject.gameObject.SetActive(m_search_bar.DropdownIconObject.GetComponent<Image>().sprite != null);
                ApplyFilter();
            });

            m_search_bar.InputField.onValueChanged.AddListener(delegate (string _) { ApplyFilter(); });

            if (Main.Settings.SearchBarScrollResetOnSubmit)
            {
                m_search_bar.InputField.onSubmit.AddListener(delegate (string _)
                {
                    transform.Find(PathToStashScroll()).GetComponent<Scrollbar>().value = 0.0f;
                });
            }

            m_char_selection_changed_cb = Game.Instance.SelectionCharacter.SelectedUnit.Subscribe(delegate (UnitDescriptor _) { ApplyFilter(); });

            // Add options to the dropdown...

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

            m_search_bar.Dropdown.AddOptions(options);

            // Gather images for the dropdown...

            List<Image> images = new List<Image>();
            GameObject switch_bar = m_filter_block.Find("SwitchBar").gameObject;

            foreach (Transform child in switch_bar.transform)
            {
                images.Add(child.Find("Icon")?.GetComponent<Image>());
            }

            while (images.Count < options.Count)
            {
                images.Add(null);
            }

            m_search_icons = images.ToArray();

            // Tweak positioning depending on user config...

            RectTransform search_transform = m_search_bar.GameObject.GetComponent<RectTransform>();

            if (Main.Settings.SearchBarEnableCategoryButtons)
            {
                search_transform.localScale = new Vector3(0.6f, 0.6f, 1.0f);
                search_transform.localPosition = new Vector3(0.0f, -8.0f, 0.0f);

                RectTransform sb_transform = switch_bar.GetComponent<RectTransform>();
                sb_transform.localPosition = new Vector3(
                    sb_transform.localPosition.x,
                    sb_transform.localPosition.y + 23.0f,
                    sb_transform.localPosition.z);
                sb_transform.localScale = new Vector3(0.6f, 0.6f, 1.0f);

                // destroy the top and bottom gfx as they cause a lot of noise
                Destroy(m_search_bar.GameObject.transform.Find("Background/Decoration/TopLineImage").gameObject);
                Destroy(m_search_bar.GameObject.transform.Find("Background/Decoration/BottomLineImage").gameObject);
            }
            else
            {
                search_transform.localScale = new Vector3(0.85f, 0.85f, 1.0f);
                search_transform.localPosition = new Vector3(0.0f, 2.0f, 0.0f);
                Destroy(switch_bar);
            }
        }

        private void OnEnable()
        {
            m_active_filter = null;
        }

        private void OnDestroy()
        {
            m_char_selection_changed_cb.Dispose();
        }

        private void Update()
        {
            if (m_active_filter == null)
            {
                switch (Type)
                {
                    case InventoryType.InventoryStash:
                        InventoryStashPCView stash_pc_view = GetComponentInParent<InventoryStashPCView>();
                        m_active_filter = stash_pc_view.ViewModel.ItemsFilter.CurrentFilter;
                        stash_pc_view.ViewModel.ItemSlotsGroup.CollectionChangedCommand.Subscribe(delegate (bool _) { ApplyFilter(); });
                        stash_pc_view.ViewModel.ItemsFilter.CurrentSorter.Subscribe(delegate (ItemsFilter.SorterType _) { ApplyFilter(); });
                        break;

                    case InventoryType.Vendor:
                        VendorPCView vendor_pc_view = GetComponentInParent<VendorPCView>();
                        m_active_filter = vendor_pc_view.ViewModel.VendorItemsFilter.CurrentFilter;
                        vendor_pc_view.ViewModel.VendorSlotsGroup.CollectionChangedCommand.Subscribe(delegate (bool _) { ApplyFilter(); });
                        vendor_pc_view.ViewModel.VendorItemsFilter.CurrentSorter.Subscribe(delegate (ItemsFilter.SorterType _) { ApplyFilter(); });
                        break;

                    case InventoryType.LootCollector:
                        LootCollectorPCView collector_pc_view = GetComponentInParent<LootCollectorPCView>();
                        m_active_filter = collector_pc_view.ViewModel.ItemsFilter.CurrentFilter;
                        collector_pc_view.ViewModel.CollectionChangedCommand.Subscribe(delegate (Unit _) { ApplyFilter(); });
                        collector_pc_view.ViewModel.ItemsFilter.CurrentSorter.Subscribe(delegate (ItemsFilter.SorterType _) { ApplyFilter(); });
                        break;

                    case InventoryType.LootInventoryStash:
                        LootInventoryStashPCView inventory_pc_view = GetComponentInParent<LootInventoryStashPCView>();
                        m_active_filter = inventory_pc_view.ViewModel.ItemsFilter.CurrentFilter;
                        inventory_pc_view.ViewModel.ItemSlotsGroup.CollectionChangedCommand.Subscribe(delegate (bool _) { ApplyFilter(); });
                        inventory_pc_view.ViewModel.ItemsFilter.CurrentSorter.Subscribe(delegate (ItemsFilter.SorterType _) { ApplyFilter(); });
                        break;
                }

                Transform switch_bar = m_filter_block.Find("SwitchBar");

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
                            toggle.onValueChanged.AddListener(delegate (bool on){ if (on) m_search_bar.Dropdown.value = mapped_idx; });
                        }
                    }
                }

                if (Type == InventoryType.InventoryStash || Type == InventoryType.LootInventoryStash)
                {
                    if (Main.Settings.SearchBarResetFilterWhenOpeningInv)
                    {
                        m_search_bar.Dropdown.value = Main.FilterMapper.From((int)ItemsFilter.FilterType.NoFilter);
                    }

                    if (Main.Settings.SearchBarFocusWhenOpeningInv)
                    {
                        m_search_bar.FocusSearchBar();
                    }
                }
            }
        }

        private string PathToFilterBlock()
        {
            switch (Type)
            {
                case InventoryType.LootCollector: return "Filters/Filters";
                case InventoryType.LootInventoryStash: return "Filters/PC_FilterBlock/Filters";
            }

            return "PC_FilterBlock/Filters";
        }

        private string PathToStashScroll()
        {
            switch (Type)
            {
                case InventoryType.InventoryStash: return "StashScrollView/Scrollbar Vertical";
                case InventoryType.Vendor: return "VendorStashScrollView/Scrollbar Vertical";
                case InventoryType.LootCollector: return "Collector/StashScrollView/Scrollbar Vertical";
                case InventoryType.LootInventoryStash: return "Stash/StashScrollView/Scrollbar Vertical";
            }

            return null;
        }

        private void ApplyFilter()
        {
            if (m_active_filter != null)
            {
                Hooks.ItemsFilter_ShouldShowItem_Blueprint.SearchContents = m_search_bar.InputField.text;
                m_active_filter.SetValueAndForceNotify((ItemsFilter.FilterType)Main.FilterMapper.To(m_search_bar.Dropdown.value));
                Hooks.ItemsFilter_ShouldShowItem_Blueprint.SearchContents = null;
            }
        }
    }
}