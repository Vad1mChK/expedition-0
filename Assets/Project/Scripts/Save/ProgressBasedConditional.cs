using System;
using System.Collections.Generic;
using System.Linq;
using Expedition0.Items.Data;
using Expedition0.Save.Experimental;
using Expedition0.Util;
using NaughtyAttributes;
using UnityEngine;

namespace Expedition0.Save
{
    [Serializable]
    public class ProgressBasedConditional<T>
    {
        public enum ConditionKind
        {
            [Tooltip("Completed levels (list of tags)")] CompletedLevels,
            [Tooltip("Collected unique inventory items (list of tags)")] InventoryItems,
            [Tooltip("Collected the specific inventory item (tag)")] InventoryItem,
            [Tooltip("Number of solved tasks (int)")] TasksSolvedCount,
            [Tooltip("Number of collected unique artifacts (int)")] ArtifactCount,
        }

        public enum ComparisonMode { IntComparison, SetComparison }

        [Header("Target")]
        public ConditionKind kind;
        
        [Header("Int Conditions")]
        [Tooltip("Used for Int-based stats (Deaths, Tasks, etc)")]
        public int targetInt;
        public ProgressBasedConditionalComparison intOp;

        [Header("String Set Conditions")]
        [Tooltip("Used for Set-based checks (Levels, Inventory)")]
        // [ShowIf($"{nameof(kind)} == 0 || {nameof(kind)} == 1")]
        public List<string> targetStrings;
        public SetUtils.SetComparison setOp;

        [Header("String Conditions")]
        [Tooltip("Used for String-based checks (Single inventory item)")]
        public string targetString;

        [Header("Outcome")]
        public T outcome;

        public bool IsSatisfied() => IsSatisfied(PlaythroughLifecycleManager.Instance.CurrentData);

        private Func<string, ItemData.ItemType?> itemTypeGetter =
            (itemId => PlaythroughLifecycleManager.Instance?.itemRegistry?.GetItem(itemId)?.itemType);

        public bool IsSatisfied(PlaythroughSaveData data)
        {
            return kind switch
            {
                ConditionKind.CompletedLevels => SetUtils.CompareSets(data.completedLevels, targetStrings, setOp),
                ConditionKind.InventoryItems => SetUtils.CompareSets(data.inventory.Select(i => i.itemId), targetStrings, setOp),
                ConditionKind.InventoryItem => data.inventory.Any(item => item.itemId == targetString),
                ConditionKind.TasksSolvedCount => CompareInts(data.taskSolvedCount, targetInt, intOp),
                ConditionKind.ArtifactCount => CompareInts(
                    data.CountUniqueItems(ItemData.ItemType.Artifact, itemTypeGetter), targetInt, intOp
                ),
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