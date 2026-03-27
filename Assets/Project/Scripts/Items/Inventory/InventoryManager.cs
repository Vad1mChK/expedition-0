using System;
using System.Collections.Generic;
using Expedition0.Items.Data;
using UnityEngine;

namespace Expedition0.Items.Inventory
{
    public sealed class InventoryManager : MonoBehaviour
    {
        [Serializable]
        public sealed class InventoryItem
        {
            [SerializeField] private ItemData data;
            [SerializeField] private int count;

            public ItemData Data => data;
            public string ItemId => data != null ? data.itemId : string.Empty;
            public int Count => count;
            public bool IsStackable => data != null && data.isStackable;

            public InventoryItem(ItemData data, int count)
            {
                this.data = data;
                this.count = count;
            }

            public void Add(int amount)
            {
                count = checked(count + amount);
            }

            public bool TryRemove(int amount)
            {
                if (amount <= 0) return false;
                if (count < amount) return false;
                count -= amount;
                return true;
            }
        }

        public static InventoryManager Instance { get; private set; }

        private readonly Dictionary<string, InventoryItem> _itemsById = new();
        private readonly List<string> _orderedIds = new();

        public event Action Changed;
        public event Action<ItemData> ItemAdded;

        public int DistinctItemCount => _orderedIds.Count;
        public IReadOnlyList<string> OrderedItemIds => _orderedIds;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public int WrapIndex(int index)
        {
            int n = _orderedIds.Count;
            if (n == 0) return 0;

            int m = index % n;
            if (m < 0) m += n;
            return m;
        }

        public bool TryGetItem(string itemId, out InventoryItem item) =>
            _itemsById.TryGetValue(itemId, out item);

        public bool TryGetItemByOrderIndex(int index, out InventoryItem item)
        {
            item = null;
            if (_orderedIds.Count == 0) return false;
            if (index < 0 || index >= _orderedIds.Count) return false;

            string id = _orderedIds[index];
            return _itemsById.TryGetValue(id, out item);
        }

        public bool CanAdd(ItemData data, int amount = 1)
        {
            if (!ValidateItemData(data, amount, out string itemId)) return false;

            if (_itemsById.TryGetValue(itemId, out var existing))
            {
                if (existing.Data != data)
                {
                    Debug.LogError(
                        $"Inventory itemId collision: '{itemId}' already maps to '{existing.Data.name}', attempted '{data.name}'.");
                    return false;
                }

                return data.isStackable;
            }

            return true;
        }

        public bool TryAdd(ItemData data, int amount = 1)
        {
            if (!ValidateItemData(data, amount, out string itemId)) return false;

            if (_itemsById.TryGetValue(itemId, out var existing))
            {
                if (existing.Data != data)
                {
                    Debug.LogError(
                        $"Inventory itemId collision: '{itemId}' already maps to '{existing.Data.name}', attempted '{data.name}'.");
                    return false;
                }

                if (!data.isStackable) return false;

                existing.Add(amount);
                Changed?.Invoke();
                ItemAdded?.Invoke(data);
                return true;
            }

            int initialCount = data.isStackable ? amount : 1;
            _itemsById[itemId] = new InventoryItem(data, initialCount);
            _orderedIds.Add(itemId);

            Debug.Log($"InventoryManager: Successfully added {itemId} * {amount} to inventory ");

            Changed?.Invoke();
            ItemAdded?.Invoke(data);
            return true;
        }

        public bool TryRemove(string itemId, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return false;
            if (amount <= 0) return false;

            if (!_itemsById.TryGetValue(itemId, out var item)) return false;
            if (amount > item.Count) return false;

            if (!item.TryRemove(amount)) return false;

            if (item.Count == 0)
            {
                _itemsById.Remove(itemId);
                _orderedIds.Remove(itemId);
            }

            Changed?.Invoke();
            return true;
        }
        
        public int GetCount(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return 0;

            string key = itemId.Trim();
            return _itemsById.TryGetValue(key, out var item) ? item.Count : 0;
        }


        private static bool ValidateItemData(ItemData data, int amount, out string itemId)
        {
            itemId = null;

            if (data == null) return false;
            if (amount <= 0) return false;

            if (string.IsNullOrWhiteSpace(data.itemId))
            {
                Debug.LogError($"ItemData '{data.name}' has an empty itemId.");
                return false;
            }

            itemId = data.itemId.Trim();
            return true;
        }
    }
}
