using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Expedition0.Visuals
{
    public class VisualEffectsController : MonoBehaviour
    {
        [SerializeField] private Volume volume;

        [Header("Fade to black")] [SerializeField]
        private float fadeMaxExposureDrop = -10f; // how dark at full fade

        [SerializeField] private float fadeMaxSaturationDrop = -100f; // fully desaturated

        [Header("Damage LUT")] [SerializeField]
        private float maxLutContribution = 1f;

        private ColorLookup _lut;
        private ColorAdjustments _colorAdj;

        private float _damage01; // 0..1
        private float _fade01; // 0..1

        private void Awake()
        {
            if (!volume)
                volume = GetComponent<Volume>();

            if (!volume || !volume.profile)
            {
                Debug.LogError("VisualEffectsController: Volume/profile missing!", this);
                enabled = false;
                return;
            }

            volume.profile.TryGet(out _lut);
            volume.profile.TryGet(out _colorAdj);

            // Ensure volume is fully applied, we blend inside the overrides
            volume.weight = 1f;
        }

        /// <summary>
        /// damage01 = 0 (healthy) .. 1 (near death)
        /// Only affects LUT / yellowish color cast.
        /// </summary>
        public void SetDamage01(float damage01)
        {
            _damage01 = Mathf.Clamp01(damage01);
            Apply();
        }

        /// <summary>
        /// fade01 = 0 (normal) .. 1 (fully black)
        /// Only affects brightness / saturation.
        /// </summary>
        public void SetFade01(float fade01)
        {
            _fade01 = Mathf.Clamp01(fade01);
            Apply();
        }

        private void Apply()
        {
            // Damage → LUT contribution only
            if (_lut != null) _lut.contribution.value = _damage01 * maxLutContribution;

            // Fade → exposure & saturation only
            if (_colorAdj != null)
            {
                // At fade01 = 0 → 0 exposure / saturation shift
                // At fade01 = 1 → full dark & grey
                _colorAdj.postExposure.value =
                    Mathf.Lerp(0f, fadeMaxExposureDrop, _fade01);

                _colorAdj.saturation.value =
                    Mathf.Lerp(0f, fadeMaxSaturationDrop, _fade01);
            }
        }
    }
}