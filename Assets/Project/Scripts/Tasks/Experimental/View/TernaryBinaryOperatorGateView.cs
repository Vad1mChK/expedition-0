using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Expedition0.Tasks.Experimental
{
    public sealed class TernaryBinaryOperatorGateView : TernaryBinaryOperatorNodeView
    {
        [Header("Visual Prefabs")]
        [SerializeField] private List<TernaryBinaryOperatorPrefabPair> initialGatePrefabs;

        private Dictionary<TernaryBinaryOperatorType, GameObject> _gatePrefabs;
        private GameObject _currentVisual;

        private void Awake()
        {
            _gatePrefabs = initialGatePrefabs.ToDictionary(p => p.op, p => p.prefab);
        }

        public override void UpdateView()
        {
            base.UpdateView();

            if (_gatePrefabs == null || Model == null || Model is not TernaryBinaryOperatorNode node)
                return;

            var op = node.op;

            if (!_gatePrefabs.TryGetValue(op, out var prefab))
                return;

            ReplaceVisual(prefab);
        }

        private void ReplaceVisual(GameObject prefab)
        {
            // Remove old
            if (_currentVisual != null)
            {
                Destroy(_currentVisual);
                _currentVisual = null;
            }

            // Instantiate new
            if (prefab != null)
            {
                _currentVisual = Instantiate(prefab, transform);
                _currentVisual.transform.localPosition = Vector3.zero;
                _currentVisual.transform.localRotation = Quaternion.identity;
                _currentVisual.transform.localScale = Vector3.one;
            }
        }
    }
}