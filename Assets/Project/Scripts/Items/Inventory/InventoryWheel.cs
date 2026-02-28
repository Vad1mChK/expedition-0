using UnityEngine;
using System.Collections.Generic;

namespace Expedition0.Items.Inventory
{
    public class InventoryWheel : MonoBehaviour
    {
        [SerializeField] private InventoryManager manager;
        [SerializeField] private Transform[] slots; // Exactly 5 transforms in the UI
        
        private List<GameObject> _activeIcons = new List<GameObject>();

        public void Refresh()
        {
            // Clear old icons
            foreach (var icon in _activeIcons) Destroy(icon);
            _activeIcons.Clear();

            if (!manager.HasItems) return;

            int count = manager.OwnedItems.Count;
            int current = manager.CurrentIndex;

            // Offset indices for the 5 slots: -2, -1, 0, 1, 2
            for (int i = 0; i < 5; i++)
            {
                int itemIndex = (current + (i - 2) + count) % count;
                
                // If we have few items, avoid showing duplicates in the ring 
                // unless the user specifically wants the loop visible
                if (count < 5 && i > count) continue; 

                var data = manager.OwnedItems[itemIndex];
                if (data.inventoryPrefab != null)
                {
                    GameObject icon = Instantiate(data.inventoryPrefab, slots[i]);
                    icon.transform.localPosition = Vector3.zero;
                    
                    // Highlight the middle slot (index 2)
                    icon.transform.localScale = (i == 2) ? Vector3.one * 1.2f : Vector3.one * 0.8f;
                    _activeIcons.Add(icon);
                }
            }
        }
    }
}