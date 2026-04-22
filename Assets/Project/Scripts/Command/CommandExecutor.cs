using System;
using System.Collections.Generic;
using Expedition0.Audio;
using Expedition0.Save.Experimental;
using Expedition0.Tasks.Experimental.View;
using NaughtyAttributes;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Audio;

namespace Expedition0.Command
{
    public class CommandExecutor : MonoBehaviour
    {
        [Header("Level References")]
        [SerializeField] private LevelLocalManager levelLocalManager;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private HintAndTaskExposer hintAndTaskExposer;
        
        [Header("Settings for Volume")]
        [SerializeField, Range(5, 25)] private int volumeStep = 20;
        [SerializeField] private AudioMixer audioMixer;

        [Header("Response Display")]
        [SerializeField] private CommandResponseDisplay responseDisplay;

        public LevelLocalManager LevelManager => levelLocalManager;
        public HintAndTaskExposer HintExposer => hintAndTaskExposer;

        private void Start()
        {
            if (levelLocalManager == null) levelLocalManager = FindAnyObjectByType<LevelLocalManager>();
            if (levelLocalManager == null)
            {
                Debug.LogWarning("[<b>CommandExecutor</b>]: LevelLocalManager not found in scene. Please set it in inspector.");
            }
            
            if (responseDisplay == null) responseDisplay = FindAnyObjectByType<CommandResponseDisplay>();
            if (responseDisplay == null)
            {
                Debug.LogWarning("[<b>CommandExecutor</b>]: CommandResponseDisplay not found in scene. Please set it in inspector.");
            }
        }

        public void Execute(CommandResponseDto response, AudioClip audioClip)
        {
            audioSource?.PlayOneShot(audioClip);

            var cmd = response?.command;
            if (cmd == null) return;

            var args = cmd?.recognizedArgs;

            switch (response.command.opcode)
            {
                case CommandOpcode.HINT_NEAREST:
                    if (hintAndTaskExposer.HintCount > 0)
                    {
                        var player = FindAnyObjectByType<XROrigin>()?.gameObject;
                        if (player != null)
                        {
                            var nearest = hintAndTaskExposer.GetNearestHintClientToPlayer(player);
                            nearest?.GetHint();
                        }
                    }
                    break;
                
                case CommandOpcode.HINT_TRUTHTABLE:
                    // if (args != null)
                    // {
                    //     var truthtableOperator = (string) args.GetValueOrDefault("operator", "Xor");
                    //     var truthtableBalanced = (bool) args.GetValueOrDefault("balanced", false);
                    // }
                    break;
                
                case CommandOpcode.SETTINGS_VOLUME:
                    if (args != null)
                    {
                        var volumeGroup = (string) args.GetValueOrDefault("group", "master");
                        var volumeAction = (string) args.GetValueOrDefault("action", "increase");
                        HandleSettingsVolumeCommand(volumeGroup, volumeAction);
                    }
                    break;
                
                case CommandOpcode.PROGRESS_LEVEL:
                    break;
                
                case CommandOpcode.FACT_RANDOM:
                    if (args != null)
                    {
                        var factTarget = cmd.recognizedArgs.GetValueOrDefault("target", "lore");
                    }
                    break;
                
                case CommandOpcode.UNKNOWN:
                default:
                    break;
            }
            
            responseDisplay?.DisplayCommandResult(
                response.command.opcode,
                recognizedText: response.recognizedText,
                responseText: response.responseText,
                recognizedArgs: response.command.recognizedArgs
            );
        }

        private void HandleSettingsVolumeCommand(string group, string action)
        {
            if (audioMixer == null)
            {
                Debug.LogWarning("[<b>CommandExecutor</b>]: Cannot execute SETTINGS_VOLUME command: audioMixer is null");
                return;
            }

            var volumeKey = group switch
            {
                "master" => AudioVolumeUtility.MasterKey,
                "music" => AudioVolumeUtility.MusicKey,
                "sfx" => AudioVolumeUtility.SfxKey,
                "voice" => AudioVolumeUtility.VoiceKey,
                _ => AudioVolumeUtility.MasterKey
            };
            switch (action)
            {
                case "increase": 
                    AudioVolumeUtility.AdjustVolume(audioMixer, volumeKey, volumeStep);
                    break;
                case "decrease":
                    AudioVolumeUtility.AdjustVolume(audioMixer, volumeKey, -volumeStep);
                    break;
                case "mute":
                    AudioVolumeUtility.MuteVolume(audioMixer, volumeKey);
                    break;
            }
        }
    }
}