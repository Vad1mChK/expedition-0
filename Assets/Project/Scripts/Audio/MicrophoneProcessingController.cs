using Expedition0.Items.ItemsHeld;
using UnityEngine;

namespace Expedition0.Audio
{
    public sealed class MicrophoneProcessingController : MonoBehaviour
    {
        [SerializeField] private MicrophoneHeld microphoneHeld;

        private void OnEnable()
        {
            if (microphoneHeld != null)
                microphoneHeld.RecordingReady += HandleRecordingReady;
        }

        private void OnDisable()
        {
            if (microphoneHeld != null)
                microphoneHeld.RecordingReady -= HandleRecordingReady;
        }

        private void HandleRecordingReady(MicrophoneHeld.RecordedAudio recAudio)
        {
            // 1) Do your async processing/network request elsewhere.
            // 2) When finished, call:
            // microphoneHeld.NotifyCommunicationResult(success: true/false);

            // Placeholder: always succeed
            
            microphoneHeld.NotifyCommunicationResult(false);
        }
    }
}