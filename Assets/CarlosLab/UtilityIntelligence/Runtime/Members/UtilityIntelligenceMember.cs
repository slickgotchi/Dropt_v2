using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public class UtilityIntelligenceMember
    {
        private UtilityIntelligence intelligence;

        public UtilityIntelligence Intelligence
        {
            get => intelligence;
            internal set
            {
                if (intelligence == value) return;

                intelligence = value;

                OnIntelligenceChanged(intelligence);
            }
        }

        public UtilityAgent Agent => intelligence?.Agent;
        public IEntityFacade AgentFacade => Agent?.EntityFacade;
        
        protected virtual void OnIntelligenceChanged(UtilityIntelligence intelligence)
        {
            
        }
    }
}