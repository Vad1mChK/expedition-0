using NUnit.Framework;
using Expedition0.Tasks.Experimental;

namespace Expedition0.Test.Tasks.Model
{
    public class TestLogicNodes
    {
        #region Value Node Tests

        [Test]
        public void TernaryValueNode_Cycle_ChangesValueWhenNotLocked()
        {
            // Arrange
            var node = new TernaryValueNode { currentValue = Trit.False, locked = false };

            // Act
            node.Cycle();

            // Assert
            Assert.AreEqual(Trit.Neutral, node.currentValue, "Ternary node should cycle to next Trit when unlocked.");
        }

        [Test]
        public void TernaryValueNode_Cycle_DoesNotChangeValueWhenLocked()
        {
            // Arrange
            var node = new TernaryValueNode { currentValue = Trit.False, locked = true };

            // Act
            node.Cycle();

            // Assert
            Assert.AreEqual(Trit.False, node.currentValue, "Locked node should not change value on Cycle.");
        }

        [Test]
        public void NonaryValueNode_Cycle_WrapsCorrectlyAtNine()
        {
            // Arrange
            var node = new NonaryValueNode { currentValue = 8, locked = false };

            // Act
            node.Cycle();

            // Assert
            Assert.AreEqual(0, node.currentValue, "Nonary node should wrap from 8 back to 0.");
        }

        #endregion

        #region Operator Node Tests

        [Test]
        public void TernaryUnaryOperatorNode_Evaluate_ReturnsCorrectLogic()
        {
            // Arrange
            var valNode = new TernaryValueNode { currentValue = Trit.True };
            var opNode = new TernaryUnaryOperatorNode 
            { 
                input = valNode, 
                op = TernaryUnaryOperatorType.Not 
            };

            // Act & Assert
            // Not(True) -> False
            Assert.AreEqual(Trit.False, opNode.EvaluateTrit());
        }

        [Test]
        public void TernaryBinaryOperatorNode_Evaluate_CombinesInputs()
        {
            // Arrange
            var left = new TernaryValueNode { currentValue = Trit.True };
            var right = new TernaryValueNode { currentValue = Trit.False };
            var opNode = new TernaryBinaryOperatorNode
            {
                leftInput = left,
                rightInput = right,
                op = TernaryBinaryOperatorType.And
            };

            // Act & Assert
            // True AND False in most ternary systems -> False
            Assert.AreEqual(Trit.False, opNode.EvaluateTrit());
        }

        [Test]
        public void NonaryOperatorNode_Concat_CalculatesCorrectBaseNine()
        {
            // Arrange
            // Let's simulate: (1 * 9) + 2 = 11
            var left = new NonaryValueNode { currentValue = 1 };
            var right = new NonaryValueNode { currentValue = 2 };
            var opNode = new NonaryOperatorNode
            {
                leftInput = left,
                rightInput = right,
                op = NonaryOperatorType.NonaryConcat
            };

            // Act
            int result = opNode.EvaluateInt();

            // Assert
            Assert.AreEqual(11, result);
        }

        #endregion

        #region Integration / Complexity Tests

        [Test]
        public void NestedNodes_Evaluate_CalculatesDeepLogicTree()
        {
            // Arrange: (Not(True) AND True)
            var valA = new TernaryValueNode { currentValue = Trit.True };
            var valB = new TernaryValueNode { currentValue = Trit.True };
            
            var notNode = new TernaryUnaryOperatorNode 
            { 
                input = valA, 
                op = TernaryUnaryOperatorType.Not 
            };

            var andNode = new TernaryBinaryOperatorNode
            {
                leftInput = notNode,
                rightInput = valB,
                op = TernaryBinaryOperatorType.And
            };

            // Act: Not(True) is False. False AND True is False.
            Trit finalResult = andNode.EvaluateTrit();

            // Assert
            Assert.AreEqual(Trit.False, finalResult);
        }

        #endregion
    }
}