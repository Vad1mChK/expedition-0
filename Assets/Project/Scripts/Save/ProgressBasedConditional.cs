using System;
using UnityEngine;

namespace Expedition0.Save
{
    [Serializable]
    public class ProgressBasedConditional<T>
    {
        public enum ProgressBasedConditionalKind
        {
            MainLevelCount,
            GameProgressMask
        }

        public enum ProgressBasedConditionalComparison
        {
            Less,
            LessOrEqual,
            Equal,
            GreaterOrEqual,
            Greater,
            NotEqual
        }

        [Header("Condition")]
        public ProgressBasedConditionalKind kind;
        public ProgressBasedConditionalComparison comparison;

        [Tooltip("Used if kind == MainLevelCount")]
        public int intValue;

        [Tooltip("Used if kind == GameProgressMask")]
        public GameProgress maskValue;

        [Header("Outcome")]
        public T outcome;
        
        // Public API

        public bool SatisfiedForCurrentProgress() =>
            SatisfiedFor(SaveManager.LoadProgress());

        public bool SatisfiedFor(GameProgress progress)
        {
            switch (kind)
            {
                case ProgressBasedConditionalKind.MainLevelCount:
                    int count = SaveManager.MainLevelsCompletedCount((int)progress);
                    return CompareInts(count, intValue, comparison);

                case ProgressBasedConditionalKind.GameProgressMask:
                    bool hasFlags = (progress & maskValue) == maskValue;
                    return CompareProgress(progress, maskValue, comparison);

                default:
                    return false;
            }
        }
        
        // Helpers
        
        private static bool CompareInts(int a, int b, ProgressBasedConditionalComparison op)
        {
            return op switch
            {
                ProgressBasedConditionalComparison.Less          => a <  b,
                ProgressBasedConditionalComparison.LessOrEqual   => a <= b,
                ProgressBasedConditionalComparison.Equal         => a == b,
                ProgressBasedConditionalComparison.GreaterOrEqual=> a >= b,
                ProgressBasedConditionalComparison.Greater       => a >  b,
                ProgressBasedConditionalComparison.NotEqual      => a != b,
                _ => false
            };
        }

        private static bool CompareProgress(GameProgress a, GameProgress b, ProgressBasedConditionalComparison op)
        {
            return op switch
            {
                ProgressBasedConditionalComparison.Less => (b & a) == a && ((int)b & (int)~a) != 0,
                ProgressBasedConditionalComparison.LessOrEqual => (b & a) == a,
                ProgressBasedConditionalComparison.Equal => a == b,
                ProgressBasedConditionalComparison.GreaterOrEqual => (a & b) == b,
                ProgressBasedConditionalComparison.Greater => (a & b) == b && ((int)a & (int)~b) != 0,
                ProgressBasedConditionalComparison.NotEqual => a != b,
                _ => false
            };
        }
    }
}