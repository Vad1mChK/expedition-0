using System;
using UnityEngine;

namespace Expedition0.Environment
{
    public class UVRegionSelector : MonoBehaviour
    {
        [SerializeField] private Vector2 scale = new Vector2(1f, 1f);
        [SerializeField] private Vector2 offset = new Vector2(0f, 0f);
        [SerializeField] private MeshRenderer meshRenderer;
        
        // Property names for URP/Lit shader
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }
        }
        
        private void OnValidate()
        {
            if (!meshRenderer) return;
            
            ApplyUVSettings();
        }

        public void ApplyUVSettings()
        {
            if (!meshRenderer) return;

            foreach (var material in meshRenderer.materials)
            {
                // For URP/Lit shader, we need to set the tiling and offset on the _BaseMap property
                // Using Material.SetTextureOffset and SetTextureScale
                material.SetTextureOffset(BaseMap, offset);
                material.SetTextureScale(BaseMap, scale);
                
                // Alternative approach using material.mainTextureOffset/Scale (works for standard shaders too)
                // material.mainTextureOffset = offset;
                // material.mainTextureScale = scale;
            }
        }

        // Public methods to change values at runtime
        public void SetScale(Vector2 newScale)
        {
            scale = newScale;
            ApplyUVSettings();
        }

        public void SetOffset(Vector2 newOffset)
        {
            offset = newOffset;
            ApplyUVSettings();
        }

        public void SetUVSettings(Vector2 newScale, Vector2 newOffset)
        {
            scale = newScale;
            offset = newOffset;
            ApplyUVSettings();
        }

        // Getters for current values
        public Vector2 GetScale() => scale;
        public Vector2 GetOffset() => offset;
    }
}