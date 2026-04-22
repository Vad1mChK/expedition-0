using System.Collections.Generic;

namespace Expedition0.Util
{
    public static class SetUtils
    {
        public enum SetComparison
        {
            StrictSubset,  // <
            Subset,  // <=
            Equal,  // ==
            Superset,  // >=
            StrictSuperset,  // >
            NotEqual,  // !=
        }
        
        public static bool CompareSets<T>(IEnumerable<T> current, IEnumerable<T> target, SetComparison op)
        {
            var thisSet = new HashSet<T>(current);
            var otherSet = new HashSet<T>(target);

            return op switch
            {
                SetComparison.Equal => thisSet.SetEquals(otherSet),
                SetComparison.NotEqual => !thisSet.SetEquals(otherSet),
                
                // Current must contain only items from target, and be smaller
                SetComparison.StrictSubset => thisSet.IsProperSubsetOf(otherSet),
                
                // Current must contain only items from target
                SetComparison.Subset => thisSet.IsSubsetOf(otherSet),
                
                // Current must contain all items from target, and be larger
                SetComparison.StrictSuperset => thisSet.IsProperSupersetOf(otherSet),
                
                // Current must contain all items from target
                SetComparison.Superset => thisSet.IsSupersetOf(otherSet),
                
                _ => false
            };
        }
    }
}