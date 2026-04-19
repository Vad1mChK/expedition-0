using System.Collections.Generic;

namespace Expedition0.Command
{
    public enum CommandOpcode
    {
        HINT_NEAREST,
        HINT_TRUTHTABLE,
        SETTINGS_VOLUME,
        PROGRESS_LEVEL,
        FACT_RANDOM,
        UNKNOWN
    }

    public class CommandAudioVolumes
    {
        public int masterVolume = 100;
        public int musicVolume = 100;
        public int sfxVolume = 100;
        public int voiceVolume = 100;
    }

    public class CommandContextArgs
    {
        public string levelId;
        public List<string> completedLevelIds;
        public Dictionary<string, int> inventory;
        public int completedTaskCount;
        public int totalTaskCount;
        public int hintCount;
        public CommandAudioVolumes volumes;
    }

    public class CommandRequestDto
    {
        public CommandContextArgs contextArgs;
    }

    // Response Models
    public class BaseCommand
    {
        public CommandOpcode opcode;
        public Dictionary<string, object> recognizedArgs;
        public Dictionary<string, object> contextArgs;
    }

    public class CommandResponseDto
    {
        public string responseText;
        public string recognizedText;
        public BaseCommand command;
    }
}