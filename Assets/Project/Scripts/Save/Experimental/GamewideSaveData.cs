using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Expedition0.Save.Experimental
{
    public class GamewideSaveData : ExpeditionBaseSaveData<GamewideSaveData>
    {
        public List<string> charactersUnlocked = new();
        public List<string> musicUnlocked = new();
        public List<string> itemsUnlocked = new();
        public List<string> endingsUnlocked = new();

        private static GamewideSaveData _cachedDefault;
        private static GamewideSaveData _cachedPerfect;

        public static GamewideSaveData Default => 
            _cachedDefault ??= LoadTemplateFromResources("defaultGamewideSave");
        public static GamewideSaveData Perfect => 
            _cachedPerfect ??= LoadTemplateFromResources("perfectGamewideSave");

        public bool IsCharacterUnlocked(string id) => charactersUnlocked.Any(c => c == id);
    }
}