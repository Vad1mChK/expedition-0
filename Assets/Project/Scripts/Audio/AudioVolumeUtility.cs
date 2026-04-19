using UnityEngine;
using UnityEngine.Audio;
using System;

namespace Expedition0.Audio
{
    public static class AudioVolumeUtility
    {
        // Event for the View to subscribe to
        public static event Action OnSettingsChanged;

        public const string MasterKey = "MasterVolume";
        public const string MusicKey = "MusicVolume";
        public const string SfxKey = "SfxVolume";
        public const string VoiceKey = "VoiceVolume";

        private const float MinDb = -80f;
        private const float MaxDb = 0f;

        private const int DefaultVolume = 80;

        // persistence: 0-100 (int)
        public static int GetVolume(string key) => PlayerPrefs.GetInt(key, DefaultVolume);

        public static void SetVolume(AudioMixer mixer, string key, int value)
        {
            int clampedValue = Mathf.Clamp(value, 0, 100);
            PlayerPrefs.SetInt(key, clampedValue);
            PlayerPrefs.Save();

            ApplyToMixer(mixer, key, clampedValue);
            OnSettingsChanged?.Invoke();
        }

        public static void AdjustVolume(AudioMixer mixer, string key, int delta)
        {
            int current = GetVolume(key);
            SetVolume(mixer, key, current + delta);
        }
        
        public static void ApplyAll(AudioMixer mixer)
        {
            ApplyToMixer(mixer, MasterKey, GetVolume(MasterKey));
            ApplyToMixer(mixer, MusicKey, GetVolume(MusicKey));
            ApplyToMixer(mixer, SfxKey, GetVolume(SfxKey));
            ApplyToMixer(mixer, VoiceKey, GetVolume(VoiceKey));
        }

        public static void ApplyToMixer(AudioMixer mixer, string key, int value)
        {
            if (mixer == null) return;

            // Logarithmic conversion: 0-100 -> -80dB to 0dB
            float normalized = value / 100f;
            float dB = normalized > 0.0001f ? 20f * Mathf.Log10(normalized) : MinDb;
            
            mixer.SetFloat(key, dB);
        }

        // Wrapper methods for CommandExecutor
        
        public static int GetMaster() => GetVolume(MasterKey);
        public static int GetMusic() => GetVolume(MusicKey);
        public static int GetSfx() => GetVolume(SfxKey);
        public static int GetVoice() => GetVolume(VoiceKey);
        
        public static void SetMaster(AudioMixer m, int v) => SetVolume(m, MasterKey, v);
        public static void SetMusic(AudioMixer m, int v) => SetVolume(m, MusicKey, v);
        public static void SetSfx(AudioMixer m, int v) => SetVolume(m, SfxKey, v);
        public static void SetVoice(AudioMixer m, int v) => SetVolume(m, VoiceKey, v);

        public static void MuteVolume(AudioMixer m, string key) => SetVolume(m, key, 0);
        public static void MuteMaster(AudioMixer m) => SetVolume(m, MasterKey, 0);
        public static void MuteMusic(AudioMixer m) => SetVolume(m, MusicKey, 0);
        public static void MuteSfx(AudioMixer m) => SetVolume(m, SfxKey, 0);
        public static void MuteVoice(AudioMixer m) => SetVolume(m, VoiceKey, 0);

        public static void AdjustMaster(AudioMixer m, int v) => AdjustVolume(m, MasterKey, v);
        public static void AdjustMusic(AudioMixer m, int v) => AdjustVolume(m, MusicKey, v);
        public static void AdjustSfx(AudioMixer m, int v) => AdjustVolume(m, SfxKey, v);
        public static void AdjustVoice(AudioMixer m, int v) => AdjustVolume(m, VoiceKey, v);
    }
}