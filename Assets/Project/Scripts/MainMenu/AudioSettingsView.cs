using System;
using Expedition0.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Experimental.XR.Interaction;

namespace Expedition0.MainMenu
{
    public sealed class AudioSettingsView : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField, Range(5, 25)] private int volumeStep = 10;
        
        [Header("Text Labels for Volume Values")]
        [SerializeField] private TMP_Text masterLabel;
        [SerializeField] private TMP_Text musicLabel;
        [SerializeField] private TMP_Text sfxLabel;
        [SerializeField] private TMP_Text voiceLabel;

        private void OnEnable()
        {
            AudioVolumeUtility.OnSettingsChanged += RefreshLabels;
            RefreshLabels();
        }

        private void OnDisable()
        {
            AudioVolumeUtility.OnSettingsChanged -= RefreshLabels;
        }

        public void RefreshLabels()
        {
            UpdateLabel(masterLabel, "MasterVolume");
            UpdateLabel(musicLabel, "MusicVolume");
            UpdateLabel(sfxLabel, "SfxVolume");
            UpdateLabel(voiceLabel, "VoiceVolume");
        }

        private void UpdateLabel(TMP_Text label, string key)
        {
            if (label != null)
            {
                label.text = $"{AudioVolumeUtility.GetVolume(key)}";
            }
        }

        public void IncreaseMaster() => AudioVolumeUtility.AdjustMaster(audioMixer, volumeStep);
        public void DecreaseMaster() => AudioVolumeUtility.AdjustMaster(audioMixer, -volumeStep);
        public void IncreaseMusic() => AudioVolumeUtility.AdjustMusic(audioMixer, volumeStep);
        public void DecreaseMusic() => AudioVolumeUtility.AdjustMusic(audioMixer, -volumeStep);
        public void IncreaseSfx() => AudioVolumeUtility.AdjustSfx(audioMixer, volumeStep);
        public void DecreaseSfx() => AudioVolumeUtility.AdjustSfx(audioMixer, -volumeStep);
        public void IncreaseVoice() => AudioVolumeUtility.AdjustVoice(audioMixer, volumeStep);
        public void DecreaseVoice() => AudioVolumeUtility.AdjustVoice(audioMixer, -volumeStep);

        [ContextMenu("Print Current Audio Groups")]
        public void PrintCurrentAudioGroups()
        {
            Debug.Log($"Audio Volumes: Master {AudioVolumeUtility.GetMaster()}, " +
                      $"Music {AudioVolumeUtility.GetMusic()}, " +
                      $"Sfx {AudioVolumeUtility.GetSfx()}, " +
                      $"Voice {AudioVolumeUtility.GetVoice()}");
        }
    }
}