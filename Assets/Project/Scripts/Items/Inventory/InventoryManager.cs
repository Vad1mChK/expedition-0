using System;
using System.Collections.Generic;
using Expedition0.Items.Core;
using UnityEngine;
using Expedition0.Items.Data;

namespace Expedition0.Items.Inventory
{
    [Serializable]
    public class InventoryEntry
    {
        public string id;
        public int count;
    }

    [Serializable]
    public class InventorySaveData
    {
        public List<InventoryEntry> items = new List<InventoryEntry>();
    }

    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] private List<ItemData> database;
        [SerializeField] private Transform handPivot;
        
        private List<ItemData> _ownedItems = new List<ItemData>();
        private int _currentIndex = 0;
        private GameObject _spawnedHeldItem;
        private ItemData _lastEquippedItem;

        public List<ItemData> OwnedItems => _ownedItems;
        public int CurrentIndex => _currentIndex;
        public bool HasItems => _ownedItems.Count > 0;
        private ItemHeld _currentHeldScript; // Cache the component for performance

        public void AddItem(ItemData data)
        {
            if (!_ownedItems.Contains(data))
            {
                _ownedItems.Add(data);
                // Requirement 1: Immediately equip
                EquipItem(data);
                _currentIndex = _ownedItems.Count - 1;
            }
        }

        public void EquipItem(ItemData data)
        {
            if (_spawnedHeldItem != null) Destroy(_spawnedHeldItem);
            _currentHeldScript = null;

            if (data == null || data.heldPrefab == null) return;

            _spawnedHeldItem = Instantiate(data.heldPrefab, handPivot);
            _spawnedHeldItem.transform.localPosition = Vector3.zero;
            _spawnedHeldItem.transform.localRotation = Quaternion.identity;
    
            // Cache the ItemHeld component so we don't call GetComponent every frame
            _currentHeldScript = _spawnedHeldItem.GetComponent<ItemHeld>();
            _lastEquippedItem = data;
        }

        public void PassTriggerInput(bool pressed)
        {
            if (_currentHeldScript == null) return;
            
            if (pressed) _currentHeldScript.OnTriggerPressed();
            else _currentHeldScript.OnTriggerReleased();
        }

        public void Holster()
        {
            if (_spawnedHeldItem != null) Destroy(_spawnedHeldItem);
            _spawnedHeldItem = null;
        }

        public void ToggleHolsterLast()
        {
            if (_spawnedHeldItem != null) Holster();
            else if (_lastEquippedItem != null) EquipItem(_lastEquippedItem);
            else if (HasItems) EquipItem(_ownedItems[0]);
        }

        public void ChangeSelection(int direction)
        {
            if (!HasItems) return;
            _currentIndex = (_currentIndex + direction + _ownedItems.Count) % _ownedItems.Count;
        }
        
        public ItemData GetCurrentData() => HasItems ? _ownedItems[_currentIndex] : null;
    }
}