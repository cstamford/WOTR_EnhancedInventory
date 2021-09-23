using Kingmaker.UI.MVVM._PCView.Slots;
using Owlcat.Runtime.UniRx;
using UnityEngine;
using UniRx;
using UnityEngine.EventSystems;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._VM.Slots;
using UnityEngine.UI;
using EnhancedInventory.Settings;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Items.Parts;
using Kingmaker.Blueprints;
using Kingmaker;
using System.Linq;
using System;
using Kingmaker.UI.MVVM._PCView.Loot;
using Owlcat.Runtime.UI.VirtualListSystem;
using Kingmaker.UI.MVVM._VM.Loot;
using System.Collections.Generic;

namespace EnhancedInventory.Controllers
{
    public class LootHighlightController : MonoBehaviour
    {
        private List<IDisposable> m_handlers = new List<IDisposable>();
        private bool m_apply_handlers = true;
        private bool m_deferred_update = true;

        private void OnEnable()
        {
            m_apply_handlers = true;
        }

        private void OnTransformChildrenChanged()
        {
            m_apply_handlers = true;
        }

        private void Update()
        {
            m_deferred_update |= m_apply_handlers;

            ItemSlotsGroupView[] views = m_deferred_update ? GetComponentsInChildren<ItemSlotsGroupView>() : null;

            if (m_apply_handlers)
            {
                foreach (IDisposable handler in m_handlers)
                {
                    handler.Dispose();
                }

                m_handlers.Clear();

                // Can be null if we're in the loot window with no filters.
                ItemsFilterVM filter_vm = GetComponentInParent<LootCollectorPCView>().m_ItemsFilter.ViewModel;

                if (filter_vm != null)
                {
                    m_handlers.Add(filter_vm.CurrentFilter.Subscribe(delegate (ItemsFilter.FilterType _) { m_deferred_update = true; }));
                    m_handlers.Add(filter_vm.CurrentSorter.Subscribe(delegate (ItemsFilter.SorterType _) { m_deferred_update = true; }));
                }

                foreach (ItemSlotsGroupView view in views)
                {
                    m_handlers.Add(view.ViewModel.CollectionChangedCommand.Subscribe(delegate (bool _) { m_deferred_update = true; }));
                }

                foreach (ItemSlotPCView pc_slot in GetComponentsInChildren<ItemSlotPCView>())
                {
                    m_handlers.Add(pc_slot.OnDropAsObservable().Subscribe(delegate (PointerEventData _) { m_deferred_update = true; }));
                }

                m_apply_handlers = false;
            }

            if (m_deferred_update)
            {
                foreach (ItemSlotsGroupView view in views)
                {
                    foreach (IWidgetView widget in view.m_WidgetList.m_VisibleEntries)
                    {
                        ItemSlotView<ItemSlotVM> slot = widget as ItemSlotView<ItemSlotVM>;

                        if (slot == null) continue;

                        Transform highlight = slot.transform.Find("Item/EnhancedInventory_Highlight");

                        if (highlight == null)
                        {
                            GameObject base_obj = new GameObject("EnhancedInventory_Highlight", new Type[] { typeof(RectTransform), typeof(CanvasGroup) });

                            RectTransform transform = base_obj.GetComponent<RectTransform>();
                            transform.anchorMin = new Vector2(0.0f, 0.0f);
                            transform.anchorMax = new Vector2(1.0f, 1.0f);
                            transform.offsetMin = new Vector2(0.0f, 0.0f);
                            transform.offsetMax = new Vector2(0.0f, 0.0f);
                            transform.pivot = new Vector2(0.5f, 0.5f);

                            GameObject new_highlight_border = Instantiate(base_obj, base_obj.transform);
                            new_highlight_border.name = "HighlightBorder";
                            new_highlight_border.AddComponent<RawImage>().texture = Texture2D.whiteTexture;

                            // Hack to fix bug https://issuetracker.unity3d.com/issues/image-color-cannot-be-changed-via-script-when-image-type-is-set-to-simple
                            // Duplicating base_obj for HighlightBackground will result in a white, semi-transparent image, but duplicating new_highlight_border
                            // fixes it, which is why we have done that.

                            GameObject new_highlight_bg = Instantiate(new_highlight_border, base_obj.transform);
                            new_highlight_bg.name = "HighlightBackground";
                            new_highlight_bg.transform.localScale = new Vector3(0.9f, 0.9f, 1.0f);

                            highlight = base_obj.transform;
                            highlight.SetParent(slot.transform.Find("Item"), false);
                            highlight.SetSiblingIndex(1);
                        }

                        highlight.Find("HighlightBorder").GetComponent<RawImage>().color = Main.Settings.HighlightLootBorder;
                        highlight.Find("HighlightBackground").GetComponent<RawImage>().color = Main.Settings.HighlightLootBackground;

                        bool enabled = slot.Item != null;

                        if (enabled)
                        {
                            CopyScroll scroll = slot.Item.Blueprint.GetComponent<CopyScroll>();
                            CopyRecipe recipe = slot.Item.Blueprint.GetComponent<CopyRecipe>();
                            ItemPartShowInfoCallback cb = slot.Item.Get<ItemPartShowInfoCallback>();

                            bool is_copyable_scroll = scroll != null && Game.Instance.Player.Party.Any(i => scroll.CanCopy(slot.Item, i));
                            bool is_unlearned_recipe = recipe != null && Game.Instance.Player.Party.Any(i => recipe.CanCopy(slot.Item, i));
                            bool is_unread_document = cb != null && (!cb.m_Settings.Once || !cb.m_Triggered);

                            enabled = (Main.Settings.HighlightLootOptions.HasFlag(HighlightLootableOptions.UnlearnedScrolls) && is_copyable_scroll) ||
                                (Main.Settings.HighlightLootOptions.HasFlag(HighlightLootableOptions.UnlearnedRecipes) && is_unlearned_recipe) ||
                                (Main.Settings.HighlightLootOptions.HasFlag(HighlightLootableOptions.UnreadDocuments) && is_unread_document);
                        }

                        highlight.gameObject.SetActive(enabled);
                    }
                }

                m_deferred_update = false;
            }
        }
    }
}
