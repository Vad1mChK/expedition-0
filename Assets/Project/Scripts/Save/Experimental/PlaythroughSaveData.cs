using System;
using System.Collections.Generic;
using UnityEngine;

namespace Expedition0.Save.Experimental
{
    [Serializable]
    public class PlaythroughSaveData : ExpeditionBaseSaveData<PlaythroughSaveData>
    {
        public List<PlaythroughInventoryEntry> inventory = new();
        
        public List<string> completedLevels = new();
        public string respawnLevel = "e0:bridge_corridor";
        
        public float currentHealth = 100f;
        
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