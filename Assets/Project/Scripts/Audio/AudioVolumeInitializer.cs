using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Expedition0.Audio
{
    public class AudioVolumeInitializer : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;

        private void Start()
        {
            if (audioMixer != null)
            {
                AudioVolumeUtility.ApplyAll(audioMixer);
                Debug.Log("[<b>AudioInitializer</b>] Saved volume settings applied to Mixer.");
            }
        }
    }
}