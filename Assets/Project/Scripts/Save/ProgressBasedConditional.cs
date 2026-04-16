using System;
using System.Collections.Generic;
using System.Linq;
using Expedition0.Save.Experimental;
using Expedition0.Util;
using UnityEngine;

namespace Expedition0.Save
{
    [Serializable]
    public class ProgressBasedConditional<T>
    {
        public enum ConditionKind
        {
            CompletedLevels,
            InventoryItems,
            TasksSolved,
        }

        public enum ComparisonMode { IntComparison, SetComparison }

        [Header("Target")]
        public ConditionKind kind;
        
        [Tooltip("Used for Int-based stats (Deaths, Tasks, etc)")]
        public int targetInt;
        public ProgressBasedConditionalComparison intOp;

        [Tooltip("Used for Set-based checks (Levels, Inventory)")]
        public List<string> targetStrings;
        public SetUtils.SetComparison setOp;

        [Header("Outcome")]
        public T outcome;

        public bool IsSatisfied() => IsSatisfied(PlaythroughLifecycleManager.Instance.CurrentData);

        public bool IsSatisfied(PlaythroughSaveData data)
        {
            return kind switch
            {
                ConditionKind.CompletedLevels => SetUtils.CompareSets(data.completedLevels, targetStrings, setOp),
                ConditionKind.InventoryItems => SetUtils.CompareSets(data.inventory.Select(i => i.itemId), targetStrings, setOp),
                ConditionKind.TasksSolved => CompareInts(data.taskSolvedCount, targetInt, intOp),
                _ => false
            };
        }

        private static bool CompareInts(int a, int b, ProgressBasedConditionalComparison op)
        {
            Debug.Log($"Checking condition: {a} {op} {b}");
            return op switch
            {
                ProgressBasedConditionalComparison.Less           => a <  b,
                ProgressBasedConditionalComparison.LessOrEqual    => a <= b,
                ProgressBasedConditionalComparison.Equal          => a == b,
                ProgressBasedConditionalComparison.GreaterOrEqual => a >= b,
                ProgressBasedConditionalComparison.Greater        => a >  b,
                ProgressBasedConditionalComparison.NotEqual       => a != b,
                _ => false
            };
        }
    }

    public enum ProgressBasedConditionalComparison
    {
        Less, LessOrEqual, Equal, GreaterOrEqual, Greater, NotEqual
    }
}