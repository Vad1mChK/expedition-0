using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

namespace Expedition0.Throwaway
{
    public class EditorOnlyPrefabSummoner : MonoBehaviour
    {
        [Serializable]
        public struct PrefabSummonData
        {
            public GameObject prefab;
            public Transform summonTransform;
            [CanBeNull] public GameObject parent;
        }

        [SerializeField] private List<PrefabSummonData> prefabsToSummon;

        private void Awake()
        {
#if UNITY_EDITOR
            foreach (var prefab in prefabsToSummon)
                if (prefab.parent != null)
                    Instantiate(
                        prefab.prefab,
                        prefab.summonTransform.position,
                        prefab.summonTransform.rotation,
                        prefab.parent.transform
                    );
                else
                    Instantiate(
                        prefab.prefab,
                        prefab.summonTransform.position,
                        prefab.summonTransform.rotation
                    );
#else
            Debug.LogWarning("EditorOnlyPrefabSummoner: Not in Editor mode, won't summon anything");
#endif
        }
    }
}