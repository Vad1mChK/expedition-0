using System;
using System.Collections.Generic;
using System.Linq;
using Expedition0.Items.Data;
using Expedition0.Save.Experimental;
using Expedition0.Save.Registries;
using UnityEngine;

namespace Expedition0.Items.Inventory
{
    public sealed class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [SerializeField] private ItemRegistry itemRegistry;
        
        // This is our in-memory cache of the SaveData list for quick dictionary lookups
        private readonly Dictionary<string, PlaythroughInventoryEntry> _itemsById = new();
        private readonly List<string> _orderedIds = new();

        public event Action Changed;
        public event Action<ItemData> ItemAdded;

        public int DistinctItemCount => _orderedIds.Count;
        public IReadOnlyList<string> OrderedItemIds => _orderedIds;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            InitializeFromSave();
        }

        private void InitializeFromSave()
        {
            var saveData = PlaythroughLifecycleManager.Instance.CurrentData.inventory;
            _itemsById.Clear();
            _orderedIds.Clear();

            foreach (var entry in saveData)
            {
                _itemsById[entry.itemId] = entry;
                _orderedIds.Add(entry.itemId);
            }
            Changed?.Invoke();
        }

        public bool TryAdd(ItemData data, int amount = 1)
        {
            var saveList = PlaythroughLifecycleManager.Instance.CurrentData.inventory;
            string id = data.itemId;

            int index = saveList.FindIndex(e => e.itemId == id);

            if (index >= 0)
            {
                if (!data.isStackable) return false;
                
                var entry = saveList[index];
                entry.count += amount;
                saveList[index] = entry;
            }
            else
            {
                saveList.Add(new PlaythroughInventoryEntry { itemId = id, count = amount });
                _orderedIds.Add(id);
            }

            // Refresh our lookup and notify UI
            SyncInternalState();
            ItemAdded?.Invoke(data);
            Changed?.Invoke();
            return true;
        }

        public bool TryRemove(string itemId, int amount = 1)
        {
            var saveList = PlaythroughLifecycleManager.Instance.CurrentData.inventory;
            int index = saveList.FindIndex(e => e.itemId == itemId);
            
            if (index < 0) return false;

            var entry = saveList[index];
            if (entry.count < amount) return false;

            entry.count -= amount;
            if (entry.count <= 0)
            {
                saveList.RemoveAt(index);
                _orderedIds.Remove(itemId);
            }
            else
            {
                saveList[index] = entry;
            }

            SyncInternalState();
            Changed?.Invoke();
            return true;
        }

        private void SyncInternalState()
        {
            _itemsById.Clear();
            foreach (var entry in PlaythroughLifecycleManager.Instance.CurrentData.inventory)
            {
                _itemsById[entry.itemId] = entry;
            }
        }

        // Helpers for UI
        public int GetCount(string itemId) => _itemsById.TryGetValue(itemId, out var entry) ? entry.count : 0;
        
        public bool TryGetItemData(string itemId, out ItemData data)
        {
            data = itemRegistry.GetItem(itemId);
            return data != null;
        }

        public int WrapIndex(int index)
        {
            int n = _orderedIds.Count;
            if (n == 0) return 0;
            int m = index % n;
            return m < 0 ? m + n : m;
        }
    }
}