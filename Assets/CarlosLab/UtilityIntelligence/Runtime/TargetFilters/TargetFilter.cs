#region

using System.Collections.Generic;
using System.Runtime.Serialization;
using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public abstract class TargetFilter : UtilityIntelligenceMemberItem
    {
        #region Target Filter

        private readonly List<UtilityEntity> targetCache = new();

        public IReadOnlyList<UtilityEntity> TargetCache => targetCache;
        
        public bool FilterTarget(UtilityEntity target)
        {
            bool result = OnFilterTarget(target);
            if(result)
                targetCache.Add(target);
            return result;
        }

        protected abstract bool OnFilterTarget(UtilityEntity target);

        public void ClearCache()
        {
            targetCache.Clear();
        }

        #endregion
    }
}