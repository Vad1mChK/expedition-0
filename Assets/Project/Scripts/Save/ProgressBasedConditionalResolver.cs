using System;
using System.Collections.Generic;
using Expedition0.Save.Experimental;
using NUnit.Framework;
using UnityEngine;

namespace Expedition0.Save
{
    [Serializable]
    public class ProgressBasedConditionalResolver<T>
    {
        public List<ProgressBasedConditional<T>> conditionalValues = new();
        public T defaultValue;

        public T Resolve()
        {
            var data = PlaythroughLifecycleManager.Instance.CurrentData;
            foreach (var cond in conditionalValues)
            {
                if (cond.IsSatisfied(data)) return cond.outcome;
            }
            return defaultValue;
        }
    }
}