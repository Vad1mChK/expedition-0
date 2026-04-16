using System;
using System.Collections.Generic;
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
        public int artifactCount;

        private static PlaythroughSaveData _cachedDefault;
        
        public static PlaythroughSaveData Default => 
            _cachedDefault ??= LoadTemplateFromResources("defaultPlaythroughSave");
    }
}