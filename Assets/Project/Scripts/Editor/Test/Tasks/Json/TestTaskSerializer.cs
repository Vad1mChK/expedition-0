using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Expedition0.Tasks.Experimental;
using Expedition0.Tasks.Experimental.Json;

namespace Expedition0.Test.Tasks.Json
{
    public class TestTaskSerialization
    {
        [Test]
        public void SerializeTask_GraphWithMultipleNodes_AssignsUniqueIds()
        {
            // Arrange: Create a small tree: (TritVal) -> TritUn
            var valNode = new TernaryValueNode { currentValue = Trit.True };
            var root = new TernaryUnaryOperatorNode 
            { 
                input = valNode, 
                op = TernaryUnaryOperatorType.Not 
            };

            // Act
            var data = TaskSerializer.SerializeTask(root);

            // Assert
            Assert.AreEqual(2, data.nodes.Count);
            Assert.AreNotEqual(data.nodes[0].id, data.nodes[1].id, "Each node should have a unique assigned ID.");
            Assert.IsTrue(data.nodes.Any(n => n.type == "TritVal"));
            Assert.IsTrue(data.nodes.Any(n => n.type == "TritUn"));
        }

        [Test]
        public void SerializeTask_HandlesSharedNodes_WithoutDuplicates()
        {
            // Arrange: One value node used as BOTH inputs for a binary node
            var sharedValue = new TernaryValueNode { currentValue = Trit.Neutral };
            var root = new TernaryBinaryOperatorNode
            {
                leftInput = sharedValue,
                rightInput = sharedValue,
                op = TernaryBinaryOperatorType.Xor
            };

            // Act
            var data = TaskSerializer.SerializeTask(root);

            // Assert
            // If the traversal is correct, we should only have 2 nodes total (root + one shared)
            Assert.AreEqual(2, data.nodes.Count, "Shared nodes should only be serialized once.");
        }
    }
}