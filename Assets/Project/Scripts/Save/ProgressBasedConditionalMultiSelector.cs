using System;
using System.Collections.Generic;
using System.Linq;
using Expedition0.Save.Experimental;
using NaughtyAttributes;
using NUnit.Framework;
using UnityEngine;

namespace Expedition0.Save
{
    [Serializable]
    public class ProgressBasedConditionalMultiSelector<T>
    {
        public List<ProgressBasedConditional<T>> conditionalValues = new();

        public List<T> Select()
        {
            var data = PlaythroughLifecycleManager.Instance.CurrentData;
            return conditionalValues
                .Where(v => v.IsSatisfied(data))
                .Select(v => v.outcome)
                .ToList();
        }
    }
}