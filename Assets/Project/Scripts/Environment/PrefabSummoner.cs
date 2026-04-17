using System;
using System.Collections;
using System.Collections.Generic;
using Expedition0.Save;
using NaughtyAttributes;
using UnityEngine;

namespace Expedition0.Environment
{
    public class PrefabSummoner : MonoBehaviour
    {
        [Serializable]
        protected class PrefabSummonerEntry
        {
            public GameObject prefab;
            public Transform transform;
            public bool useTransformAsParent = true;
            public bool hasLifetime = false;
            [ShowIf(nameof(hasLifetime), true)] public float lifetime = 5f;
        }

        [Serializable]
        protected struct LifetimeMonitorEntry
        {
            public GameObject gameObject;
            public float lifetimeLeft;

            public LifetimeMonitorEntry(GameObject go, float lifetime)
            {
                gameObject = go;
                lifetimeLeft = lifetime;
            }
        }

        [SerializeField] private Transform defaultTransform;
        [SerializeField] protected ProgressBasedConditionalResolver<List<PrefabSummonerEntry>> summonResolver = new();
        [SerializeField] private bool summonAllOnAwake;
        [SerializeField] private bool destroyAllOnDestroy = true;

        private readonly List<LifetimeMonitorEntry> _lifetimeMonitorList = new();
        private Coroutine _lifetimeMonitorRoutine;

        private void Awake()
        {
            defaultTransform ??= transform;

            if (summonAllOnAwake)
            {
                SummonAllResolved();
            }
        }

        public virtual void SummonAllResolved()
        {
            foreach (var entry in summonResolver.Resolve())
            {
                SummonEntry(entry);
            }
        }
        
        public virtual void SummonAllDefault()
        {
            foreach (var entry in summonResolver.defaultValue)
            {
                SummonEntry(entry);
            }
        }

        public void SummonDefaultEntryByIndex(int idx)
        {
            var entriesToSummon = summonResolver.defaultValue;
            
            if (idx < 0 || idx >= entriesToSummon.Count)
                return;
            SummonEntry(entriesToSummon[idx]);
        }

        protected GameObject SummonEntry(PrefabSummonerEntry entry)
        {
            if (entry == null || entry.prefab == null)
            {
                Debug.LogWarning("PrefabSummoner: Cannot summon a null prefab");
                return null;
            }

            if (entry.hasLifetime && entry.lifetime <= 0f)
            {
                Debug.LogWarning($"PrefabSummoner: {entry.prefab.name} has zero or negative lifetime; will not be summoned");
                return null;
            }

            Transform summonTransform = entry.transform != null ? entry.transform : defaultTransform;

            var go = Instantiate(
                entry.prefab,
                summonTransform.position,
                summonTransform.rotation,
                entry.useTransformAsParent ? summonTransform : null
            );

            if (entry.hasLifetime)
            {
                _lifetimeMonitorList.Add(new LifetimeMonitorEntry(go, entry.lifetime));
                EnsureMonitoring();
            }

            return go;
        }

        private void EnsureMonitoring()
        {
            if (_lifetimeMonitorRoutine == null)
            {
                _lifetimeMonitorRoutine = StartCoroutine(Monitor());
            }
        }

        private void StopMonitoringAt(int index, bool destroy = true)
        {
            if (index < 0 || index >= _lifetimeMonitorList.Count)
                return;

            var entry = _lifetimeMonitorList[index];
            _lifetimeMonitorList.RemoveAt(index);

            if (destroy && entry.gameObject)
            {
                Destroy(entry.gameObject);
            }

            if (_lifetimeMonitorList.Count == 0 && _lifetimeMonitorRoutine != null)
            {
                StopCoroutine(_lifetimeMonitorRoutine);
                _lifetimeMonitorRoutine = null;
            }
        }

        private void OnDestroy()
        {
            if (_lifetimeMonitorRoutine != null)
            {
                StopCoroutine(_lifetimeMonitorRoutine);
                _lifetimeMonitorRoutine = null;
            }

            if (destroyAllOnDestroy)
            {
                foreach (var monitorEntry in _lifetimeMonitorList)
                {
                    Destroy(monitorEntry.gameObject);
                }
            }
        }

        private IEnumerator Monitor()
        {
            while (true)
            {
                float dt = Time.deltaTime;

                // Iterate backwards so RemoveAt is safe.
                for (int i = _lifetimeMonitorList.Count - 1; i >= 0; i--)
                {
                    var entry = _lifetimeMonitorList[i];

                    // In case someone destroyed it externally:
                    if (!entry.gameObject)
                    {
                        StopMonitoringAt(i, destroy: false);
                        continue;
                    }

                    entry.lifetimeLeft -= dt;

                    if (entry.lifetimeLeft <= 0f)
                    {
                        StopMonitoringAt(i, destroy: true);
                        continue;
                    }

                    // IMPORTANT: write the modified struct back into the list
                    _lifetimeMonitorList[i] = entry;
                }

                yield return null;
            }
        }
    }
}
