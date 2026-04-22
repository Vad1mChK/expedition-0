using Expedition0.Tasks.Experimental;
using NUnit.Framework;

namespace Expedition0.Test.Tasks.Model
{
    public class TestTernaryMath
    {
        [Test]
        [TestCase(Trit.False, Trit.False)]
        [TestCase(Trit.Neutral, Trit.Neutral)]
        [TestCase(Trit.True, Trit.True)]
        public void Identity_ForAllTrits_IsCorrect(Trit input, Trit result)
        { 
            Assert.AreEqual(TernaryMath.Identity(input), result);
        }

        [Test]
        [TestCase(Trit.False, Trit.True)]
        [TestCase(Trit.Neutral, Trit.Neutral)]
        [TestCase(Trit.True, Trit.False)]
        public void Not_ForAllTrits_IsCorrect(Trit input, Trit result)
        {
            Assert.AreEqual(TernaryMath.Not(input), result);
        }
        
        [Test]
        [TestCase(Trit.False, Trit.True)]
        [TestCase(Trit.Neutral, Trit.False)]
        [TestCase(Trit.True, Trit.Neutral)]
        public void CyclePrevious_ForAllTrits_IsCorrect(Trit input, Trit result)
        {
            Assert.AreEqual(TernaryMath.CyclePrevious(input), result);
        }
        
        [Test]
        [TestCase(Trit.False, Trit.Neutral)]
        [TestCase(Trit.Neutral, Trit.True)]
        [TestCase(Trit.True, Trit.False)]
        public void CycleNext_ForAllTrits_IsCorrect(Trit input, Trit result)
        {
            Assert.AreEqual(TernaryMath.CycleNext(input), result);
        }
        
        [Test]
        [TestCase(Trit.False, Trit.False, Trit.False)]
        [TestCase(Trit.False, Trit.Neutral, Trit.False)]
        [TestCase(Trit.False, Trit.True, Trit.False)]
        [TestCase(Trit.Neutral, Trit.Neutral, Trit.Neutral)]
        [TestCase(Trit.Neutral, Trit.True, Trit.Neutral)]
        [TestCase(Trit.True, Trit.True, Trit.True)]
        public void And_ForTritPairs_IsCorrect(Trit left, Trit right, Trit result)
        {
            Assert.AreEqual(TernaryMath.And(left, right), result);
        }
        
        [Test]
        [TestCase(Trit.False, Trit.False, Trit.False)]
        [TestCase(Trit.False, Trit.Neutral, Trit.Neutral)]
        [TestCase(Trit.False, Trit.True, Trit.True)]
        [TestCase(Trit.Neutral, Trit.Neutral, Trit.Neutral)]
        [TestCase(Trit.Neutral, Trit.True, Trit.True)]
        [TestCase(Trit.True, Trit.True, Trit.True)]
        public void Or_ForTritPairs_IsCorrect(Trit left, Trit right, Trit result)
        {
            Assert.AreEqual(TernaryMath.Or(left, right), result);
        }
        
        [Test]
        [TestCase(Trit.False, Trit.False, Trit.False)]
        [TestCase(Trit.False, Trit.Neutral, Trit.Neutral)]
        [TestCase(Trit.False, Trit.True, Trit.True)]
        [TestCase(Trit.Neutral, Trit.Neutral, Trit.Neutral)]
        [TestCase(Trit.Neutral, Trit.True, Trit.Neutral)]
        [TestCase(Trit.True, Trit.True, Trit.False)]
        public void Xor_ForTritPairs_IsCorrect(Trit left, Trit right, Trit result)
        {
            Assert.AreEqual(TernaryMath.Xor(left, right), result);
        }
        
        [Test]
        [TestCase(Trit.False, Trit.False, Trit.True)]
        [TestCase(Trit.False, Trit.True, Trit.True)]
        [TestCase(Trit.Neutral, Trit.False, Trit.Neutral)]
        [TestCase(Trit.Neutral, Trit.Neutral, Trit.Neutral)]
        [TestCase(Trit.Neutral, Trit.True, Trit.True)]
        [TestCase(Trit.True, Trit.False, Trit.False)]
        public void ImplKleene_ForTritPairs_IsCorrect(Trit left, Trit right, Trit result)
        {
            Assert.AreEqual(TernaryMath.ImplKleene(left, right), result);
        }
        
        [Test]
        [TestCase(Trit.False, Trit.False, Trit.True)]
        [TestCase(Trit.False, Trit.True, Trit.True)]
        [TestCase(Trit.Neutral, Trit.False, Trit.Neutral)]
        [TestCase(Trit.Neutral, Trit.Neutral, Trit.True)]
        [TestCase(Trit.Neutral, Trit.True, Trit.True)]
        [TestCase(Trit.True, Trit.False, Trit.False)]
        public void ImplLukasiewicz_ForTritPairs_IsCorrect(Trit left, Trit right, Trit result)
        {
            Assert.AreEqual(TernaryMath.ImplLukasiewicz(left, right), result);
        }

        [Test]
        [TestCase(Trit.False, Trit.False, 0)]
        [TestCase(Trit.False, Trit.Neutral, 1)]
        [TestCase(Trit.False, Trit.True, 2)]
        [TestCase(Trit.Neutral, Trit.Neutral, 4)]
        [TestCase(Trit.True, Trit.True, 8)]
        public void ToNonary_ForTritPairs(Trit high, Trit low, int result)
        {
            Assert.AreEqual(TernaryMath.ToNonary(high, low), result);
        }
    }
}