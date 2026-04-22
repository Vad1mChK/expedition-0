using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Expedition0.Tasks.Experimental
{
    public class TernaryUnaryOperatorGateView : TernaryUnaryOperatorNodeView
    {
        [Header("Visual Prefabs")]
        [SerializeField] private List<TernaryUnaryOperatorPrefabPair> initialGatePrefabs;

        private Dictionary<TernaryUnaryOperatorType, GameObject> _gatePrefabs;
        private GameObject _currentVisual;
        
        private void Awake()
        {
            if (Model == null) BuildModel();
            _gatePrefabs = initialGatePrefabs.ToDictionary(p => p.op, p => p.prefab);
            UpdateView();
        }

        public override void UpdateView()
        {
            base.UpdateView();

            if (_gatePrefabs == null || Model == null || Model is not TernaryUnaryOperatorNode node)
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