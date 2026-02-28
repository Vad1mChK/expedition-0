using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Expedition0.Save
{
    [Serializable]
    public class ProgressBasedConditionalResolver<T>
    {
        public List<ProgressBasedConditional<T>> conditionalValues = new List<ProgressBasedConditional<T>>();
        public T defaultValue;

        public T ResolveForCurrentProgress() => ResolveFor(SaveManager.LoadProgress());

        public T ResolveFor(GameProgress progress)
        {
            foreach (var value in conditionalValues)
            {
                if (value.SatisfiedFor(progress)) return value.outcome;
            }

            return defaultValue;
        }
    }
}