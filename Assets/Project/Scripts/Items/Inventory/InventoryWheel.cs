using System.Collections.Generic;
using Expedition0.Items.Data;
using Expedition0.Items.Inventory;
using UnityEngine;

namespace Expedition0.Items.UI
{
    public sealed class InventoryWheel : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private InventoryManager inventory;

        [Header("Window")]
        [Min(0)] [SerializeField] private int previewRadius = 2; // 2 => 5 slots
        [SerializeField] private float slotRadius = 0.12f;
        [SerializeField] private float angleStepDeg = 18f;

        [Header("Holograms")]
        [SerializeField] private Transform hologramStashRoot;
        [SerializeField] private float hologramScale = 1f;

        private readonly List<Transform> _slotAnchors = new();
        private readonly Dictionary<string, GameObject> _hologramById = new();
        private readonly Dictionary<string, InventoryHologram> _hologramUiById = new();

        private string[] _slotItemIds;
        private int _selectedIndex;

        private int SlotCount => previewRadius * 2 + 1;

        private void Awake()
        {
            if (inventory == null) inventory = InventoryManager.Instance;

            if (hologramStashRoot == null)
            {
                var stash = new GameObject("HologramStash");
                stash.transform.SetParent(transform, false);
                hologramStashRoot = stash.transform;
            }

            RebuildSlots();
        }

        private void OnEnable()
        {
            if (inventory != null) inventory.Changed += HandleInventoryChanged;
            HandleInventoryChanged();
        }

        private void OnDisable()
        {
            if (inventory != null) inventory.Changed -= HandleInventoryChanged;
            // ClearSlots();
        }

        public void MoveSelection(int delta)
        {
            if (inventory == null || inventory.DistinctItemCount == 0) return;

            _selectedIndex = inventory.WrapIndex(_selectedIndex + delta);
            Refresh();
        }

        public bool TryGetSelectedItem(out InventoryManager.InventoryItem item)
        {
            item = null;
            if (inventory == null) return false;
            return inventory.TryGetItemByOrderIndex(_selectedIndex, out item);
        }
        
        public bool SelectItemById(string itemId)
        {
            if (inventory == null) return false;
            if (string.IsNullOrWhiteSpace(itemId)) return false;

            int n = inventory.DistinctItemCount;
            if (n == 0) return false;

            string key = itemId.Trim();

            int foundIndex = -1;
            var ids = inventory.OrderedItemIds;
            for (int i = 0; i < ids.Count; i++)
            {
                if (ids[i] == key)
                {
                    foundIndex = i;
                    break;
                }
            }

            if (foundIndex < 0) return false;

            _selectedIndex = inventory.WrapIndex(foundIndex);

            // Only refresh visuals if the wheel is currently active/enabled.
            if (isActiveAndEnabled)
                Refresh();

            return true;
        }

        private void HandleInventoryChanged()
        {
            if (inventory == null) return;

            _selectedIndex = inventory.WrapIndex(_selectedIndex);
            EnsureHologramsExist();
            Refresh();
        }

        private void RebuildSlots()
        {
            for (int i = 0; i < _slotAnchors.Count; i++)
            {
                if (_slotAnchors[i] != null)
                    Destroy(_slotAnchors[i].gameObject);
            }

            _slotAnchors.Clear();

            int slots = SlotCount;
            _slotItemIds = new string[slots];

            for (int i = 0; i < slots; i++)
            {
                int offset = i - previewRadius;

                var go = new GameObject($"WheelSlot_{offset:+0;-0;0}");
                var t = go.transform;
                t.SetParent(transform, false);

                float angleDeg = offset * angleStepDeg;
                Quaternion rot = Quaternion.AngleAxis(angleDeg, Vector3.up);
                Vector3 pos = rot * (Vector3.forward * slotRadius);

                t.localPosition = pos;
                t.localRotation = Quaternion.identity;

                _slotAnchors.Add(t);
            }
        }

        private void EnsureHologramsExist()
        {
            if (inventory == null) return;

            // Add missing
            var ids = inventory.OrderedItemIds;
            for (int i = 0; i < ids.Count; i++)
            {
                string id = ids[i];
                if (_hologramById.ContainsKey(id)) continue;

                if (!inventory.TryGetItem(id, out var item) || item.Data == null) continue;
                if (item.Data.inventoryPrefab == null)
                {
                    Debug.LogError($"Item '{id}' has no inventoryPrefab.");
                    continue;
                }

                var inst = Instantiate(item.Data.inventoryPrefab, hologramStashRoot);
                inst.name = $"Hologram_{id}";
                inst.transform.localScale = Vector3.one * hologramScale;
                inst.SetActive(false);

                _hologramById[id] = inst;
                _hologramUiById[id] = inst.GetComponentInChildren<InventoryHologram>(true);
            }

            // Remove stale
            // (Inventory is small; O(n^2) is fine. Keep it simple.)
            var toRemove = new List<string>();
            foreach (var kvp in _hologramById)
            {
                if (!inventory.TryGetItem(kvp.Key, out _))
                    toRemove.Add(kvp.Key);
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                string id = toRemove[i];
                if (_hologramById.TryGetValue(id, out var inst) && inst != null)
                    Destroy(inst);

                _hologramById.Remove(id);
                _hologramUiById.Remove(id);
            }
        }

        private void Refresh()
        {
            ClearSlots();

            if (inventory == null) return;

            int n = inventory.DistinctItemCount;
            if (n == 0) return;

            _selectedIndex = inventory.WrapIndex(_selectedIndex);

            int slots = SlotCount;
            int toShow = Mathf.Min(n, slots);

            // Offsets in display order: 0, -1, +1, -2, +2, ...
            int placed = 0;
            int step = 0;
            while (placed < toShow)
            {
                int offset;
                if (step == 0) offset = 0;
                else offset = (step % 2 == 1) ? -((step + 1) / 2) : (step / 2);

                int slotIndex = offset + previewRadius;
                if (slotIndex >= 0 && slotIndex < slots)
                {
                    int itemIndex = inventory.WrapIndex(_selectedIndex + offset);
                    if (inventory.TryGetItemByOrderIndex(itemIndex, out var item) && item?.Data != null)
                    {
                        AssignSlot(slotIndex, item);
                        placed++;
                    }
                }

                step++;
                if (step > slots * 2) break; // safety
            }
        }

        private void AssignSlot(int slotIndex, InventoryManager.InventoryItem item)
        {
            string id = item.ItemId;
            if (string.IsNullOrWhiteSpace(id)) return;
            if (!_hologramById.TryGetValue(id, out var holo) || holo == null) return;

            var anchor = _slotAnchors[slotIndex];

            _slotItemIds[slotIndex] = id;

            holo.transform.SetParent(anchor, false);
            holo.SetActive(true);

            if (_hologramUiById.TryGetValue(id, out var ui) && ui != null)
            {
                bool showCount = item.IsStackable && item.Count > 1;
                ui.SetCount(item.Count, showCount);
            }
        }

        private void ClearSlots()
        {
            if (_slotItemIds == null) return;

            for (int i = 0; i < _slotItemIds.Length; i++)
            {
                string id = _slotItemIds[i];
                if (string.IsNullOrWhiteSpace(id)) continue;

                if (_hologramById.TryGetValue(id, out var holo) && holo != null)
                {
                    holo.transform.SetParent(hologramStashRoot, false);
                    holo.SetActive(false);
                }

                _slotItemIds[i] = null;
            }
        }
        
        public bool TryGetSelectedItemData(out ItemData data)
        {
            data = null;

            // Replace this with your actual selection retrieval
            if (!TryGetSelectedItem(out var selectedItem)) return false;
            if (selectedItem == null || selectedItem.Data == null) return false;

            data = selectedItem.Data;
            return true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                // Keep editor behavior predictable: only rebuild when values are valid.
                if (previewRadius < 0) previewRadius = 0;
            }
        }
#endif
    }
}
