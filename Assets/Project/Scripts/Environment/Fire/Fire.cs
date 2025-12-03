using System;
using System.Collections.Generic;
using Expedition0.Health;
using Unity.VisualScripting;
using UnityEngine;

namespace Expedition0.Environment.Fire
{
    public class Fire : MonoBehaviour
    {
        [Header("Flame visuals")] public GameObject flamePrefab; // your flipbook quad/particle prefab
        public Transform[] flameAnchors; // if empty -> this.transform

        [Header("Lights (one per anchor, created on demand)")]
        public Light lightPrefab; // small point light (no shadows)

        public Transform[] lightAnchors; // if empty -> flameAnchors (or this)
        public Color lightColor = new(1f, 0.65f, 0.2f);
        public float lightIntensity = 1.6f;
        public float lightRange = 3.5f;

        public bool igniteOnStart;
        public float damage = 5f;

        private readonly List<GameObject> _flames = new();
        private readonly List<Light> _lights = new();

        public bool IsActive { get; private set; }
        public IReadOnlyList<Light> Lights => _lights; // consumed by NearestFireLights

        private void Start()
        {
            if (igniteOnStart) Ignite();
        }

        public void Ignite()
        {
            if (IsActive || !flamePrefab) return;

            var fAnchors = (flameAnchors != null && flameAnchors.Length > 0) ? flameAnchors : new[] { transform };
            foreach (var a in fAnchors)
                _flames.Add(Instantiate(flamePrefab, a.position, a.rotation, transform));

            EnsureLightsCreated(); // may create new lights

            // <-- re-activate and (re)register existing lights
            foreach (var L in _lights)
            {
                if (!L) continue;
                L.gameObject.SetActive(true);
                L.enabled = false;           // manager will fade it up
                L.intensity = 0f;
                NearestFireLights.Instance?.RegisterLight(L);
            }

            IsActive = true;
        }

        public void Extinguish()
        {
            if (!IsActive) return;

            foreach (var go in _flames) if (go) Destroy(go);
            _flames.Clear();

            // <-- hide from manager and unregister
            foreach (var L in _lights)
            {
                if (!L) continue;
                L.enabled = false;
                L.intensity = 0f;
                L.gameObject.SetActive(false);                // excludes it from candidates
                NearestFireLights.Instance?.UnregisterLight(L);
            }

            IsActive = false;
        }

        private void EnsureLightsCreated()
        {
            if (!lightPrefab) return;

            var anchors = lightAnchors != null && lightAnchors.Length > 0
                ? lightAnchors
                : flameAnchors != null && flameAnchors.Length > 0
                    ? flameAnchors
                    : new[] { transform };

            if (_lights.Count == anchors.Length) return; // already built

            foreach (var L in _lights)
                if (L)
                    Destroy(L.gameObject);
            _lights.Clear();

            foreach (var a in anchors)
            {
                var L = Instantiate(lightPrefab, a.position, a.rotation, transform);
                L.type = LightType.Point;
                L.shadows = LightShadows.None; // mobile-VR friendly
                L.color = lightColor;
                L.intensity = lightIntensity;
                L.range = lightRange;
                L.enabled = false; // manager will toggle
                _lights.Add(L);
                if (NearestFireLights.Instance != null)
                {
                    NearestFireLights.Instance.RegisterLight(L);
                }
                else
                {
                    Debug.Log("Cannot register light, NearestFireLights instance is null");
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var l in _lights)
            {
                if (NearestFireLights.Instance != null)
                {
                    NearestFireLights.Instance.UnregisterLight(l);
                }
                else
                {
                    Debug.Log("Cannot unregister light, NearestFireLights instance is null");
                }
            }
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            var fAnchors = flameAnchors != null && flameAnchors.Length > 0
                ? flameAnchors
                : new[] { transform };
            foreach (var a in fAnchors) Gizmos.DrawWireSphere(a.position, 0.1f);

            Gizmos.color = Color.cyan;
            var lAnchors = lightAnchors != null && lightAnchors.Length > 0 ? lightAnchors : fAnchors;
            foreach (var a in lAnchors) Gizmos.DrawWireCube(a.position, Vector3.one * 0.08f);
        }
#endif
    }
}