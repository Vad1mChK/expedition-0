using System;
using System.Collections.Generic;
using UnityEngine;

namespace Expedition0.Save.Registries
{
    public class AssetRegistry<T>: ScriptableObject, ISerializationCallbackReceiver
    {
        [Serializable]
        public struct AssetEntry<T>
        {
            public string identifier;
            public T asset;
        }

        public List<AssetEntry<T>> assetEntries = new();
        public Dictionary<string, T> _cache = new();

        public T GetItem(string identifier)
        {
            return _cache.GetValueOrDefault(identifier);
        }

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            _cache.Clear();
            foreach (var entry in assetEntries)
            {
                if (string.IsNullOrEmpty(entry.identifier)) continue;
                _cache[entry.identifier] = entry.asset;
            }
        }
    }
}