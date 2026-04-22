using System.Collections.Generic;
using Expedition0.Items.Data;
using Expedition0.Items.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Expedition0.Items.UI
{
    public sealed class InventoryWheel : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private InventoryManager inventory;

        [Header("Animation Settings")]
        [SerializeField] private float lerpSpeed = 10f;
        [SerializeField] private float inactiveScale = 0.2f;
        [SerializeField] private float activeScale = 1.0f;
        [SerializeField] private float angleStepDeg = 25f;
        [SerializeField] private float radius = 0.15f;
        
        [Header("Text")]
        [SerializeField] private TMP_Text itemNameTextElement;

        private readonly Dictionary<string, GameObject> _holograms = new();
        private int _selectedIndex = 0;

        private void OnEnable() => RefreshVisuals(true);

        public void MoveSelection(int delta)
        {
            _selectedIndex = inventory.WrapIndex(_selectedIndex + delta);
            
            // Set text to name of item
            if (TryGetSelectedItemData(out var data))
            {
                UpdateText(data.itemName);
            }
        }

        private void Update()
        {
            UpdateHologramTransforms();
        }

        private void UpdateHologramTransforms()
        {
            var ids = inventory.OrderedItemIds;
            int total = ids.Count;

            for (int i = 0; i < total; i++)
            {
                string id = ids[i];
                if (!_holograms.TryGetValue(id, out var holo)) continue;

                // Calculate relative distance from selection (-2, -1, 0, 1, 2)
                int diff = i - _selectedIndex;
                
                // Wrap difference for circularity
                if (diff > total / 2) diff -= total;
                else if (diff < -total / 2) diff += total;

                // Determine Target Position and Scale
                float angle = diff * angleStepDeg * Mathf.Deg2Rad;
                Vector3 targetPos = new Vector3(Mathf.Sin(angle) * radius, 0, Mathf.Cos(angle) * radius);
                
                // Scale based on distance (0 = full size, 1+ = small/invisible)
                float distanceFactor = Mathf.Clamp01(1f - (Mathf.Abs(diff) / 3f)); 
                float targetScale = Mathf.Lerp(inactiveScale, activeScale, distanceFactor);

                // Smoothly Lerp
                holo.transform.localPosition = Vector3.Lerp(holo.transform.localPosition, targetPos, Time.deltaTime * lerpSpeed);
                holo.transform.localScale = Vector3.Lerp(holo.transform.localScale, Vector3.one * targetScale, Time.deltaTime * lerpSpeed);
                
                // Toggle visibility
                holo.SetActive(distanceFactor > 0.1f);
            }
        }

        private void RefreshVisuals(bool forceSnap = false)
        {
            // Ensure holograms exist for all items in inventory
            foreach (var id in inventory.OrderedItemIds)
            {
                if (!_holograms.ContainsKey(id))
                {
                    if (inventory.TryGetItemData(id, out var data))
                    {
                        var holo = Instantiate(data.inventoryPrefab, transform, false);
                        _holograms[id] = holo;
                        if (forceSnap) holo.transform.localScale = Vector3.zero;
                        
                        // UpdateText(data.itemName);
                    }
                }
            }
        }
        
        public bool TryGetSelectedItemData(out ItemData data)
        {
            data = null;
            if (inventory.OrderedItemIds.Count == 0) return false;
            string id = inventory.OrderedItemIds[_selectedIndex];
            return inventory.TryGetItemData(id, out data);
        }
        
        public bool SelectItemById(string itemId)
        {
            if (inventory == null) return false;
    
            var ids = inventory.OrderedItemIds;
            for (int i = 0; i < ids.Count; i++)
            {
                if (ids[i] == itemId)
                {
                    _selectedIndex = i;
                    return true;
                }
            }

            Debug.LogWarning($"[InventoryWheel] Item {itemId} not found in ordered list.");
            return false;
        }

        private void UpdateText(string text)
        {
            if (itemNameTextElement) itemNameTextElement.text = text;
        }
    }
}