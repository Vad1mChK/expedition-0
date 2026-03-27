using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Expedition0.Tasks.Experimental.Json
{
    public static class TaskSerializer
    {
        public static TaskSerializationData SerializeTask(LogicNode root)
        {
            var allNodes = new List<LogicNode>();
            var visited = new HashSet<LogicNode>();
            
            void Traverse(LogicNode node)
            {
                if (node == null || visited.Contains(node)) return;
                visited.Add(node);
                allNodes.Add(node);

                if (node is TernaryUnaryOperatorNode u) Traverse(u.input);
                if (node is TernaryBinaryOperatorNode b) { Traverse(b.leftInput); Traverse(b.rightInput); }
                if (node is NonaryOperatorNode n) { Traverse(n.leftInput); Traverse(n.rightInput); }
            }

            Traverse(root);

            for (int i = 0; i < allNodes.Count; i++) allNodes[i].id = i;

            return new TaskSerializationData
            {
                rootId = root.id,
                nodes = allNodes.Select(MapToDto).ToList()
            };
        }

        private static LogicNodeDto MapToDto(LogicNode node) => node switch
        {
            TernaryValueNode tv => new TernaryValueNodeDto 
                { id = tv.id, type = "TritVal", val = (int)tv.currentValue, locked = tv.locked },
            TernaryUnaryOperatorNode tu => new TernaryUnaryOperatorNodeDto 
                { id = tu.id, type = "TritUn", op = tu.op.ToString(), inputId = tu.input.id, locked = tu.locked },
            TernaryBinaryOperatorNode tb => new TernaryBinaryOperatorNodeDto 
                { id = tb.id, type = "TritBin", op = tb.op.ToString(), leftId = tb.leftInput.id, rightId = tb.rightInput.id, locked = tb.locked },
            NonaryValueNode nv => new NonaryValueNodeDto 
                { id = nv.id, type = "NonVal", val = nv.currentValue, locked = nv.locked },
            NonaryOperatorNode nb => new NonaryBinaryOperatorNodeDto 
                { id = nb.id, type = "NonBin", op = nb.op.ToString(), leftId = nb.leftInput.id, rightId = nb.rightInput.id, locked = nb.locked },
            _ => throw new NotSupportedException($"Unsupported node type: {node.GetType()}")
        };
    }

    [Serializable]
    public class TaskSerializationData
    {
        public int rootId;
        [JsonConverter(typeof(LogicNodeConverter))] // Forces Newtonsoft to use our logic
        public List<LogicNodeDto> nodes;
    }

    // --- DTO Hierarchy (The Discriminated Union) ---

    [Serializable]
    public abstract class LogicNodeDto
    {
        public int id;
        public string type; // Discriminator
        public bool locked;
    }

    public class TernaryValueNodeDto : LogicNodeDto { public int val; }
    public class TernaryUnaryOperatorNodeDto : LogicNodeDto { public string op; public int inputId; }
    public class TernaryBinaryOperatorNodeDto : LogicNodeDto { public string op; public int leftId; public int rightId; }
    public class NonaryValueNodeDto : LogicNodeDto { public int val; }
    public class NonaryBinaryOperatorNodeDto : LogicNodeDto { public string op; public int leftId; public int rightId; }

    // --- The Converter (Handles Deserialization) ---

    public class LogicNodeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(LogicNodeDto).IsAssignableFrom(objectType);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Use the default serialization for each specific child type
            JToken t = JToken.FromObject(value);
            t.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            string type = jo["type"]?.Value<string>();

            LogicNodeDto target = type switch
            {
                "TritVal" => new TernaryValueNodeDto(),
                "TritUn"  => new TernaryUnaryOperatorNodeDto(),
                "TritBin" => new TernaryBinaryOperatorNodeDto(),
                "NonVal"  => new NonaryValueNodeDto(),
                "NonBin"  => new NonaryBinaryOperatorNodeDto(),
                _ => throw new JsonSerializationException($"Unknown type discriminator: {type}")
            };

            serializer.Populate(jo.CreateReader(), target);
            return target;
        }
    }
}