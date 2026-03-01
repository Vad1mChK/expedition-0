using System;
using System.Collections.Generic;
using System.Linq;

namespace Expedition0.Tasks.Experimental.Json
{
    public static class TaskSerializer
    {
        public static TaskSerializationData SerializeTask(LogicNode root)
        {
            var allNodes = new List<LogicNode>();
            var visited = new HashSet<LogicNode>();
            
            // Pass 1: Collect all nodes in the DAG
            void Traverse(LogicNode node)
            {
                if (node == null || visited.Contains(node)) return;
                visited.Add(node);
                allNodes.Add(node);

                // Use pattern matching to find children without changing the base class
                if (node is TernaryUnaryOperatorNode u) Traverse(u.input);
                if (node is TernaryBinaryOperatorNode b) { Traverse(b.leftInput); Traverse(b.rightInput); }
                if (node is NonaryOperatorNode n) { Traverse(n.leftInput); Traverse(n.rightInput); }
            }

            Traverse(root);

            // Assign IDs based on index for this session
            for (int i = 0; i < allNodes.Count; i++) allNodes[i].id = i;

            // Pass 2: Map to DTOs
            return new TaskSerializationData
            {
                rootId = root.id,
                nodes = allNodes.Select(MapToDto).ToList()
            };
        }

        private static LogicNodeDto MapToDto(LogicNode node) => node switch
        {
            TernaryValueNode tv => new LogicNodeDto
            {
                id = tv.id, type = "TritVal", val = (int)tv.currentValue, locked = tv.locked
            },
            NonaryValueNode nv => new LogicNodeDto
            {
                id = nv.id, type = "NonVal", val = nv.currentValue, locked = nv.locked
            },
            TernaryUnaryOperatorNode tu => new LogicNodeDto
            {
                id = tu.id, type = "TritUn", op = tu.op.ToString(), 
                inputId = tu.input.id, locked = tu.locked
            },
            TernaryBinaryOperatorNode tb => new LogicNodeDto
            {
                id = tb.id, type = "TritBin", op = tb.op.ToString(), 
                leftId = tb.leftInput.id, rightId = tb.rightInput.id, locked = tb.locked
            },
            NonaryOperatorNode nb => new LogicNodeDto
            {
                id = nb.id, type = "NonBin", op = nb.op.ToString(), 
                leftId = nb.leftInput.id, rightId = nb.rightInput.id, locked = nb.locked
            },
            _ => null
        };
    }

    [Serializable]
    public class TaskSerializationData
    {
        public int rootId;
        public List<LogicNodeDto> nodes;
    }

    [Serializable]
    public class LogicNodeDto
    {
        public int id;
        public string type;
        public string op;
        public int val;
        public int inputId = -1;
        public int leftId = -1;
        public int rightId = -1;
        public bool locked;
    }
}