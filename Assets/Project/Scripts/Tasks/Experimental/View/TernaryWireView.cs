using System.Collections.Generic;
using UnityEngine;

namespace Expedition0.Tasks.Experimental
{
    public sealed class TernaryWireView : MonoBehaviour
    {
        [Header("View")]
        [SerializeField] private List<Renderer> renderers = new();

        private Trit _trit = Trit.Neutral;

        public Trit TritValue
        {
            get => _trit;
            set
            {
                if (_trit != value)
                {
                    _trit = value;
                    UpdateView();
                }
            }
        }

        private void Awake()
        {
            if (renderers == null)
                renderers = new List<Renderer>();

            UpdateView();
        }

        public void UpdateView()
        {
            if (renderers == null) return;

            var color = TaskColorUtil.GetColorForTrit(_trit);

            foreach (var r in renderers)
            {
                if (r == null) continue;

                var mat = r.material;
                if (mat == null) continue;

                // Enable emission
                mat.EnableKeyword("_EMISSION");

                // Set emission color
                mat.SetColor("_EmissionColor", color * 2.5f);

                // Optional: also set albedo to match glow
                mat.SetColor("_Color", color);
            }
        }
    }
}