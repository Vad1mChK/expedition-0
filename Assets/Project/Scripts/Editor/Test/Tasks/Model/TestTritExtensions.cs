using System;
using NUnit.Framework;
using Expedition0.Tasks.Experimental;

namespace Expedition0.Test.Tasks.Model
{
    public class TestTritExtensions
    {
        #region Conversion Tests

        [Test]
        [TestCase(Trit.False, 0)]
        [TestCase(Trit.Neutral, 1)]
        [TestCase(Trit.True, 2)]
        public void ToUnbalancedInt_ReturnsCorrectValues(Trit input, int expected)
        {
            Assert.AreEqual(expected, input.ToUnbalancedInt());
        }

        [Test]
        [TestCase(Trit.False, -1)]
        [TestCase(Trit.Neutral, 0)]
        [TestCase(Trit.True, 1)]
        public void ToBalancedInt_ReturnsCorrectValues(Trit input, int expected)
        {
            Assert.AreEqual(expected, input.ToBalancedInt());
        }

        [Test]
        [TestCase(0, Trit.False)]
        [TestCase(1, Trit.Neutral)]
        [TestCase(2, Trit.True)]
        public void FromUnbalancedInt_ValidInput_ReturnsCorrectTrit(int input, Trit expected)
        {
            Assert.AreEqual(expected, TritExtensions.FromUnbalancedInt(input));
        }

        [Test]
        [TestCase(-1, Trit.False)]
        [TestCase(0, Trit.Neutral)]
        [TestCase(1, Trit.True)]
        public void FromBalancedInt_ValidInput_ReturnsCorrectTrit(int input, Trit expected)
        {
            Assert.AreEqual(expected, TritExtensions.FromBalancedInt(input));
        }

        [Test]
        public void FromInt_InvalidInput_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TritExtensions.FromUnbalancedInt(5));
            Assert.Throws<ArgumentOutOfRangeException>(() => TritExtensions.FromBalancedInt(-2));
        }

        #endregion

        #region Cycling Tests

        [Test]
        [TestCase(Trit.False, Trit.Neutral)]
        [TestCase(Trit.Neutral, Trit.True)]
        [TestCase(Trit.True, Trit.False)] // Wrap around
        public void CycleNext_MovesClockwise(Trit start, Trit expected)
        {
            Assert.AreEqual(expected, start.CycleNext());
        }

        [Test]
        [TestCase(Trit.True, Trit.Neutral)]
        [TestCase(Trit.Neutral, Trit.False)]
        [TestCase(Trit.False, Trit.True)] // Wrap around
        public void CyclePrevious_MovesCounterClockwise(Trit start, Trit expected)
        {
            Assert.AreEqual(expected, start.CyclePrevious());
        }

        [Test]
        public void CycleFullCircle_ReturnsToOriginal()
        {
            Trit start = Trit.False;
            Trit result = start.CycleNext().CycleNext().CycleNext();
            
            Assert.AreEqual(start, result, "Three 'Next' cycles should return to the starting Trit.");
        }

        #endregion
    }
}