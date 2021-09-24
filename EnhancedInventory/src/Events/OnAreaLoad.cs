using EnhancedInventory.Controllers;
using Kingmaker;
using Kingmaker.PubSubSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedInventory.Events
{
    public class OnAreaLoad : IAreaHandler
    {
        public void OnAreaDidLoad()
        {
            Main.RefreshRemappers();

            if (Main.Settings.EnableSearchBar)
            {
                LoadSearchBar();
            }

            if (Main.Settings.EnableHighlightableLoot)
            {
                LoadHighlightLoot();
            }

            if (Main.Settings.EnableVisualOverhaulSorting)
            {
                SetupSortingStyle();
            }
        }

        public void OnAreaBeginUnloading()
        { }

        private readonly string[] m_inventory_paths = new string[]
        {
            "ServiceWindowsPCView/InventoryView/Inventory/Stash/StashContainer/PC_FilterBlock/Filters", // in game inventory
            "VendorPCView/MainContent/PlayerStash/PC_FilterBlock/Filters", // vendor - PC side
            "VendorPCView/MainContent/VendorBlock/PC_FilterBlock/Filters", // vendor - vendor side
            "ServiceWindowsConfig/InventoryView/Inventory/Stash/StashContainer/PC_FilterBlock/Filters", // world map inventory
            "LootPCView/Window/Inventory/Filters/PC_FilterBlock/Filters", // stash - PC side
            "LootPCView/Window/Collector/Filters/Filters" // stash - loot side
        };

        private void LoadSearchBar()
        {
            foreach (string path in m_inventory_paths)
            {
                Transform filters_block_transform = Game.Instance.UI.MainCanvas.transform.Find(path);

                if (filters_block_transform != null)
                {
                    GameObject search_bar = new GameObject("EnhancedInventory");
                    search_bar.transform.SetParent(filters_block_transform, false);
                    search_bar.name = "EnhancedInventory";
                    search_bar.AddComponent<InventoryController>();
                }
            }
        }

        private void LoadHighlightLoot()
        {
            Transform stash = Game.Instance.UI.MainCanvas.transform.Find("LootPCView/Window/Collector/Collector/StashScrollView/Viewport/Content");
            if (stash != null)
            {
                stash.gameObject.AddComponent<LootHighlightController>();
            }
        }

        private void SetupSortingStyle()
        {
            foreach (string path in m_inventory_paths)
            {
                Transform filters_block_transform = Game.Instance.UI.MainCanvas.transform.Find(path);

                if (filters_block_transform != null)
                {
                    Transform content = filters_block_transform.parent.Find("Sorting/Dropdown/Template/Viewport/Content");
                    Transform item = content.Find("Item");

                    VerticalLayoutGroup group = content.GetComponent<VerticalLayoutGroup>();
                    TextMeshProUGUI item_label = item.Find("Item Label").GetComponent<TextMeshProUGUI>();
                    RectTransform item_background = item.Find("Item Background").GetComponent<RectTransform>();
                    RectTransform item_checkmark = item.Find("Item Checkmark").GetComponent<RectTransform>();
                    RectTransform item_bottom_border = item.Find("BottomBorderImage").GetComponent<RectTransform>();

                    group.spacing = 4;
                    group.padding.top = 0;
                    group.padding.bottom = 0;

                    item_label.fontSize = 16.0f;
                    item_label.horizontalAlignment = HorizontalAlignmentOptions.Center;

                    item_background.anchorMin = new Vector2(0.0f, 0.0f);
                    item_background.anchorMax = new Vector2(1.0f, 1.0f);
                    item_background.offsetMin = new Vector2(0.0f, 0.0f);
                    item_background.offsetMax = new Vector2(0.0f, 0.0f);

                    item_checkmark.anchorMin = new Vector2(0.0f, 0.0f);
                    item_checkmark.anchorMax = new Vector2(1.0f, 1.0f);
                    item_checkmark.offsetMin = new Vector2(0.0f, 0.0f);
                    item_checkmark.offsetMax = new Vector2(0.0f, 0.0f);

                    item_bottom_border.anchorMin = new Vector2(0.0f, 0.0f);
                    item_bottom_border.anchorMax = new Vector2(1.0f, 0.0f);
                    item_bottom_border.offsetMin = new Vector2(0.0f, -2.0f);
                    item_bottom_border.offsetMax = new Vector2(0.0f, 0.0f);

                    GameObject.Destroy(content.parent.Find("TopBorderImage").gameObject);
                }
            }
        }
    }
}
