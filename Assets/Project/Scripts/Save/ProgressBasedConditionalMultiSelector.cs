using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using NUnit.Framework;
using UnityEngine;

namespace Expedition0.Save
{
    [Serializable]
    public class ProgressBasedConditionalMultiSelector<T>
    {
        public List<ProgressBasedConditional<T>> conditionalValues = new List<ProgressBasedConditional<T>>();

        public List<T> SelectForCurrentProgress() => SelectFor(SaveManager.LoadProgress());

        public List<T> SelectFor(GameProgress progress)
        {
            return conditionalValues
                .Where(value => value.SatisfiedFor(progress))
                .Select(value => value.outcome)
                .ToList();
            
            // Bet Linq is optimized enough, it's not like we'll be processing thousands of values per frame
            // Sure, we can go with the straightforward solution
            // List<T> selected = new();
            // foreach (var value in conditionalValues)
            // {
            //     if (value.SatisfiedFor(progress)) selected.Add(value.outcome);
            // }
            // return selected;
        }
    }
}