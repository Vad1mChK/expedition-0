using System;
using System.Runtime.Serialization;
using Expedition0.Tasks.Experimental.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Expedition0.Tasks.Experimental.Hint
{
    [Serializable]
    public class HintRequestDto
    {
        public TaskSerializationData leftRoot;
        public TaskSerializationData rightRoot;
        public int attemptCount;
        public int mistakeCount;
        public LogicInterfaceType leftInterfaceType = LogicInterfaceType.TernaryEquation;
        public LogicInterfaceType rightInterfaceType = LogicInterfaceType.TernaryEquation;
        public bool balanced = false;
    }

    public class HintResponseMetadataDto
    {
        public string text { get; set; }
        public string sanitizedText { get; set; }
        public LogicTaskSolverState status { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum LogicInterfaceType
    {
        TernaryEquation,
        NonaryEquation,
        TernaryCircuit
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum LogicTaskSolverState
    {
        [EnumMember(Value = "SOLVED")] Solved,
        [EnumMember(Value = "SOLVABLE")] Solvable,
        [EnumMember(Value = "UNSOLVABLE")] Unsolvable,
        [EnumMember(Value = "UNKNOWN_INCOMPLETE")] UnknownIncomplete,
        [EnumMember(Value = "SOLVABLE_INCOMPLETE")] SolvableIncomplete
    }
}