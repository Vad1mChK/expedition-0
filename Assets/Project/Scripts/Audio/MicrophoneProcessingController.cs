using System;
using System.Linq;
using System.Collections.Generic;
using Expedition0.Command;
using Expedition0.Items.ItemsHeld;
using Expedition0.Save.Experimental;
using UnityEngine;
using UnityEngine.Audio;

namespace Expedition0.Audio
{
    public sealed class MicrophoneProcessingController : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private MicrophoneHeld microphoneHeld;
        [SerializeField] private CommandClient commandClient;
        [SerializeField] private CommandExecutor commandExecutor;
        [SerializeField] private LevelLocalManager levelLocalManager;
        
        [Header("Game Context")]
        [SerializeField] private AudioMixer mainMixer;
        // In a real project, you'd reference your LevelManager or InventorySystem here

        private void Awake()
        {
            if (microphoneHeld == null)
                microphoneHeld = GetComponent<MicrophoneHeld>();

            // Auto-locate services if not manually assigned in the prefab
            if (commandClient == null)
                commandClient = FindAnyObjectByType<CommandClient>();

            if (commandExecutor == null)
                commandExecutor = FindAnyObjectByType<CommandExecutor>();

            if (levelLocalManager == null)
                levelLocalManager = FindAnyObjectByType<LevelLocalManager>();

            if (commandClient == null)
            {
                Debug.LogWarning($"[<b>MicrophoneProcessingController</b>] Failed to discover commandClient in scene.");
            }
            
            if (commandExecutor == null)
            {
                Debug.LogWarning($"[<b>MicrophoneProcessingController</b>] Failed to discover commandExecutor in scene.");
            }
            
            if (levelLocalManager == null)
            {
                Debug.LogWarning($"[<b>MicrophoneProcessingController</b>] Failed to discover commandExecutor in scene.");
            }
        }

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

        private async void HandleRecordingReady(MicrophoneHeld.RecordedAudio recAudio)
        {
            // 1. Create temporary AudioClip to use WavUtility
            AudioClip tempClip = AudioClip.Create("TempCapture", recAudio.Samples.Length / recAudio.Channels, 
                recAudio.Channels, recAudio.SampleRate, false);
            tempClip.SetData(recAudio.Samples, 0);

            // 2. Convert to Wav bytes
            byte[] wavBytes = WavUtility.FromAudioClip(tempClip);
            Destroy(tempClip); // Clean up memory

            // 3. Gather Context Data
            CommandRequestDto requestMetadata = CreateRequestDto();

            // 4. Send to Backend
            var (responseDto, audioBytes) = await commandClient.SendCommandAsync(wavBytes, requestMetadata);

            // 5. Finalize UI and Execution
            if (responseDto != null && responseDto.command != null)
            {
                microphoneHeld.NotifyCommunicationResult(success: true);
                
                Debug.Log("[<b>MicrophoneProcessingController</b>]" +
                          "HandleRecordingReady <b><color=green>Success</color></b>:" +
                          $"Received response with opcode: {responseDto.command.opcode.ToString()}, " +
                          $"responseText: {responseDto.responseText}, recognizedText: {responseDto.recognizedText}");
                
                AudioClip responseClip = null;
                if (audioBytes != null && audioBytes.Length > 0)
                {
                    // WavUtility parses the header from the bytes to Create the clip
                    responseClip = WavUtility.ToAudioClip(audioBytes);
                }
                commandExecutor.Execute(responseDto, responseClip); 
            }
            else
            {
                microphoneHeld.NotifyCommunicationResult(success: false);
                Debug.LogWarning($"[<b>MicrophoneProcessingController</b>] Failed to receive response.");
            }
        }

        private CommandRequestDto CreateRequestDto()
        {
            var currentData = PlaythroughLifecycleManager.Instance?.CurrentData;
            return new CommandRequestDto
            {
                contextArgs = new CommandContextArgs
                {
                    levelId = 
                        commandExecutor.LevelManager.LevelId,
                    completedLevelIds = 
                        currentData?.completedLevels ?? new List<string>(), 
                    inventory = 
                        currentData?.inventory?.ToDictionary(
                            elem => elem.itemId,
                            elem => elem.count
                        ) ?? new Dictionary<string, int>(),
                    volumes = GetCurrentVolumes(),
                    completedTaskCount = commandExecutor.HintExposer?.CompletedTaskCount ?? 0,
                    totalTaskCount = commandExecutor.HintExposer?.TotalTaskCount ?? 0,
                    hintCount = commandExecutor.HintExposer?.HintCount ?? 0
                }
            };
        }

        private CommandAudioVolumes GetCurrentVolumes()
        {
            return new CommandAudioVolumes
            {
                masterVolume = AudioVolumeUtility.GetMaster(),
                musicVolume = AudioVolumeUtility.GetMusic(),
                sfxVolume = AudioVolumeUtility.GetSfx(),
                voiceVolume = AudioVolumeUtility.GetVoice()
            };
        }
    }
}