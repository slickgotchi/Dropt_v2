#region

using System.Collections.Generic;
using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public sealed class UtilityAgent : UtilityEntity, IAgent
    {
        public UtilityAgent(UtilityIntelligence intelligence)
        {
            Intelligence = intelligence ?? new();
            Intelligence.Agent = this;
        }
        
        #region Properties
        public UtilityIntelligence Intelligence { get; internal set; }

        #endregion

        internal void MakeDecision(HashSet<UtilityEntity> entities)
        {
            Intelligence?.MakeDecision(entities);
        }

        internal void RunDecision(float deltaTime)
        {
            Intelligence?.RunDecision(deltaTime);
        }
    }
}