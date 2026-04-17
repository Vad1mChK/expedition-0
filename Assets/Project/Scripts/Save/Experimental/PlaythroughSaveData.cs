using System;
using System.Collections.Generic;
using System.Linq;
using Expedition0.Items.Data;
using Expedition0.Save.Registries;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

namespace Expedition0.Save.Experimental
{
    [Serializable]
    public class PlaythroughSaveData : ExpeditionBaseSaveData<PlaythroughSaveData>
    {
        public static readonly int SerialVersionId = 2026_04_14;
        public static readonly float DefaultHealth = 100f;
        
        public List<PlaythroughInventoryEntry> inventory = new();
        
        public List<string> completedLevels = new();
        public string respawnLevel;
        
        public float currentHealth = DefaultHealth;
        
        // Statistical tracking
        public int deathCount;
        public int enemyDefeatedCount;
        public int taskSolvedCount;
        public int taskMistakeCount;

        private static PlaythroughSaveData _cachedDefault;
        
        public static PlaythroughSaveData Default => 
            _cachedDefault ??= LoadTemplateFromResources("defaultPlaythroughSave");

        public int CountUniqueItems(
            ItemData.ItemType? itemType = null,
            [CanBeNull] Func<string, ItemData.ItemType?> itemTypeGetter = null
            )
        {
            if (itemType == null)
            {
                return inventory.DistinctBy(item => item.itemId).Count();
            }

            if (itemTypeGetter == null)
            {
                Debug.LogWarning("[<b>PlaythroughSaveData</b>] CountUniqueItems: Cannot get item types for items, please pass a lambda.");
                return -1;
            }

            return inventory.Count(item => itemTypeGetter(item.itemId) == itemType);
        }
    }
}