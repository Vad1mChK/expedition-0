using System;
using System.Collections;
using System.Linq;
using Expedition0.Audio;
using Expedition0.Items.Core;
using UnityEngine;

namespace Expedition0.Items.ItemsHeld
{
    public sealed class MicrophoneHeld : ItemHeld
    {
        public enum MicrophoneState
        {
            Off,
            Recording,
            Communicating,
            PlayingSuccess,
            PlayingFailure
        }

        [Serializable]
        public readonly struct RecordedAudio
        {
            public readonly float[] Samples;
            public readonly int SampleRate;
            public readonly int Channels;

            public RecordedAudio(float[] samples, int sampleRate, int channels)
            {
                Samples = samples;
                SampleRate = sampleRate;
                Channels = channels;
            }
        }

        [Header("Recording")]
        [Tooltip("Optional explicit device name. If empty, first device is used.")]
        [SerializeField] private string deviceName;
        [Min(8000)] [SerializeField] private int sampleRate = 16000;
        [Min(1)] [SerializeField] private int maxRecordingSeconds = 10;
        [Min(0)] [SerializeField] private int minRecordingMilliseconds = 200;

        [Header("State")]
        [SerializeField] private MicrophoneState state = MicrophoneState.Off;

        [Header("Visualization")]
        [Tooltip("Renderers that will receive MPB emission color updates. If empty, will auto-find in children.")]
        [SerializeField] private Renderer[] statusRenderers;
        [SerializeField] private string emissionColorProperty = "_EmissionColor";

        [ColorUsage(true, true)] [SerializeField] 
            private Color offColor = Color.black;
        [ColorUsage(true, true)] [SerializeField] 
            private Color recordingColor = new Color(0.2f, 0.8f, 2.0f, 1f);
        [ColorUsage(true, true)] [SerializeField] 
            private Color communicatingColor = new Color(2.0f, 0.8f, 0.2f, 1f);
        [ColorUsage(true, true)] [SerializeField] 
            private Color successColor = new Color(0.2f, 2.0f, 0.2f, 1f);
        [ColorUsage(true, true)] [SerializeField] 
            private Color failureColor = new Color(2.0f, 0.2f, 0.2f, 1f);

        [Tooltip("Pulse speed for Recording state.")]
        [Min(0f)] [SerializeField] private float recordingPulseHz = 2.0f;
        [Tooltip("Blink interval for Communicating state.")]
        [Min(0.01f)] [SerializeField] private float communicatingBlinkSeconds = 0.15f;

        [Header("Audio Feedback (optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip startRecordingClip;
        [SerializeField] private AudioClip stopRecordingClip;
        [SerializeField] private AudioClip successClip;
        [SerializeField] private AudioClip failureClip;

        public event Action<RecordedAudio> RecordingReady;

        public MicrophoneState State => state;

        private MaterialPropertyBlock _mpb;

        private AudioClip _recordingClip;
        private float _recordingStartRealtime;
        private Coroutine _feedbackRoutine;

        private void Awake()
        {
            MicrophonePermissions.EnsureMicrophonePermissionAndroid();
            Debug.Log("MicrophoneHeld: Microphone enabled. Available devices: [" +
                string.Join(", ", Microphone.devices) +
                "]"
            );
            
            _mpb = new MaterialPropertyBlock();

            if (statusRenderers == null || statusRenderers.Length == 0)
                statusRenderers = GetComponentsInChildren<Renderer>(true);

            ApplyStateVisual(state);
        }

        public override void OnEquip()
        {
            base.OnEquip();
            SetState(MicrophoneState.Off);
        }

        public override void OnHolster()
        {
            StopRecordingInternal(discard: true);
            SetState(MicrophoneState.Off);
            base.OnHolster();
        }

        public override void OnTriggerPressed()
        {
            base.OnTriggerPressed();

            if (state == MicrophoneState.Off || state == MicrophoneState.PlayingSuccess || state == MicrophoneState.PlayingFailure)
            {
                StartRecording();
            }
        }

        public override void OnTriggerReleased()
        {
            base.OnTriggerReleased();

            if (state == MicrophoneState.Recording)
            {
                StopRecordingInternal(discard: false);
            }
        }

        /// <summary>
        /// External systems call this when their processing/network step finishes.
        /// MicrophoneHeld does not care how you got the result.
        /// </summary>
        public void NotifyCommunicationResult(bool success)
        {
            if (state != MicrophoneState.Communicating)
                return;

            if (success)
            {
                SetState(MicrophoneState.PlayingSuccess);
                PlayOneShot(successClip);
            }
            else
            {
                SetState(MicrophoneState.PlayingFailure);
                PlayOneShot(failureClip);
            }

            // Return to Off after a short moment.
            StartCoroutine(ReturnToOffAfter(0.35f));
        }

        private void StartRecording()
        {
            StopRecordingInternal(discard: true);
            _recordingStartRealtime = Time.realtimeSinceStartup;

            if (!TryStartMic(out _recordingClip, out string dev, out int sr))
            {
                Debug.LogError("Microphone.Start failed for all attempted sample rates. Likely device format/array issue.");
                SetState(MicrophoneState.PlayingFailure);
                PlayOneShot(failureClip);
                StartCoroutine(ReturnToOffAfter(0.35f));
                return;
            }

            Debug.Log($"Mic started: device={dev}, sampleRate={sr}, lengthSec={maxRecordingSeconds}");
            SetState(MicrophoneState.Recording);
            PlayOneShot(startRecordingClip);
        }
        
        private bool TryStartMic(out AudioClip clip, out string deviceUsed, out int rateUsed)
        {
            clip = null;
            deviceUsed = null;
            rateUsed = 0;

            string device = ResolveUnityDeviceOrNull();

            // Try preferred (clamped) first
            int rate0 = ChooseSupportedSampleRate(device, sampleRate);
            clip = Microphone.Start(device, loop: false, lengthSec: maxRecordingSeconds, frequency: rate0);
            if (clip != null)
            {
                deviceUsed = device ?? "<default>";
                rateUsed = rate0;
                return true;
            }

            // Fallbacks: output sample rate, then common rates
            int[] fallbacks = { AudioSettings.outputSampleRate, 48000, 44100 };
            for (int i = 0; i < fallbacks.Length; i++)
            {
                int r = fallbacks[i];
                clip = Microphone.Start(device, loop: false, lengthSec: maxRecordingSeconds, frequency: r);
                if (clip != null)
                {
                    deviceUsed = device ?? "<default>";
                    rateUsed = r;
                    return true;
                }
            }

            return false;
        }
        
        private int ChooseSupportedSampleRate(string deviceOrNull, int preferred)
        {
            Microphone.GetDeviceCaps(deviceOrNull, out int min, out int max);

            // Unity: min==0 && max==0 means "supports any frequency". :contentReference[oaicite:5]{index=5}
            if (min == 0 && max == 0)
                return preferred > 0 ? preferred : AudioSettings.outputSampleRate;

            return Mathf.Clamp(preferred, min, max);
        }

        private void StopRecordingInternal(bool discard)
        {
            string resolvedDevice = ResolveDeviceNameOrNull();
            if (_recordingClip == null || string.IsNullOrWhiteSpace(resolvedDevice))
            {
                _recordingClip = null;
                return;
            }

            int position = Microphone.GetPosition(resolvedDevice);
            Microphone.End(resolvedDevice);

            float recordedMs = (Time.realtimeSinceStartup - _recordingStartRealtime) * 1000f;

            if (discard || position <= 0 || recordedMs < minRecordingMilliseconds)
            {
                _recordingClip = null;
                SetState(MicrophoneState.Off);
                return;
            }

            int channels = _recordingClip.channels;
            int sampleCount = position * channels;

            var samples = new float[sampleCount];

            // For non-loop recording, data starts at 0.
            _recordingClip.GetData(samples, 0);

            _recordingClip = null;

            SetState(MicrophoneState.Communicating);
            PlayOneShot(stopRecordingClip);

            RecordingReady?.Invoke(new RecordedAudio(samples, sampleRate, channels));
        }
        
        private string ResolveUnityDeviceOrNull()
        {
            if (!string.IsNullOrWhiteSpace(deviceName))
            {
                string trimmed = deviceName.Trim();
                if (Microphone.devices.Contains(trimmed)) return trimmed;

                Debug.LogWarning($"Requested mic '{trimmed}' not found. Falling back to default.");
            }

            return null; // default device
        }

        private string ResolveDeviceNameOrNull()
        {
            if (!string.IsNullOrWhiteSpace(deviceName))
                return deviceName.Trim();

            var devices = Microphone.devices;
            if (devices == null || devices.Length == 0)
                return null;

            return devices[0];
        }

        private void SetState(MicrophoneState newState)
        {
            if (state == newState) return;

            state = newState;

            if (_feedbackRoutine != null)
            {
                StopCoroutine(_feedbackRoutine);
                _feedbackRoutine = null;
            }

            ApplyStateVisual(state);

            if (state == MicrophoneState.Recording)
                _feedbackRoutine = StartCoroutine(RecordingPulseRoutine());
            else if (state == MicrophoneState.Communicating)
                _feedbackRoutine = StartCoroutine(CommunicatingBlinkRoutine());
        }

        private void ApplyStateVisual(MicrophoneState s)
        {
            Color c = s switch
            {
                MicrophoneState.Off => offColor,
                MicrophoneState.Recording => recordingColor,
                MicrophoneState.Communicating => communicatingColor,
                MicrophoneState.PlayingSuccess => successColor,
                MicrophoneState.PlayingFailure => failureColor,
                _ => offColor
            };

            SetEmissionColor(c);
        }

        private void SetEmissionColor(Color color)
        {
            if (statusRenderers == null) return;

            for (int i = 0; i < statusRenderers.Length; i++)
            {
                var r = statusRenderers[i];
                if (r == null) continue;

                r.GetPropertyBlock(_mpb);
                _mpb.SetColor(emissionColorProperty, color);
                r.SetPropertyBlock(_mpb);
            }
        }

        private IEnumerator RecordingPulseRoutine()
        {
            // Simple sine pulse between 40% and 100% intensity.
            while (state == MicrophoneState.Recording)
            {
                float t = Time.unscaledTime * (recordingPulseHz * 2f * Mathf.PI);
                float k = 0.4f + 0.6f * (0.5f + 0.5f * Mathf.Sin(t));

                SetEmissionColor(recordingColor * k);
                yield return null;
            }
        }

        private IEnumerator CommunicatingBlinkRoutine()
        {
            bool on = true;

            while (state == MicrophoneState.Communicating)
            {
                SetEmissionColor(on ? communicatingColor : offColor);
                on = !on;
                yield return new WaitForSecondsRealtime(communicatingBlinkSeconds);
            }
        }

        private IEnumerator ReturnToOffAfter(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            if (state == MicrophoneState.PlayingSuccess || state == MicrophoneState.PlayingFailure)
                SetState(MicrophoneState.Off);
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (audioSource == null || clip == null) return;
            audioSource.PlayOneShot(clip);
        }

        private void OnDisable()
        {
            // If the item gets disabled while recording, hard-stop.
            StopRecordingInternal(discard: true);
        }
    }
}