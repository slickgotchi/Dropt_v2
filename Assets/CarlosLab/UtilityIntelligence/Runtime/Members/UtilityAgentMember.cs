#region

using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public class UtilityAgentMember
    {
        private UtilityAgent agent;

        public UtilityAgent Agent
        {
            get => agent;
            internal set
            {
                if (agent == value) return;

                agent = value;

                OnAgentChanged(agent);
            }
        }


        public IEntityFacade AgentFacade => agent?.EntityFacade;

        public UtilityWorld World => agent?.World;


        protected virtual void OnAgentChanged(UtilityAgent agent)
        {
        }

        public T GetComponent<T>()
        {
            return agent != null ? agent.GetComponent<T>() : default;
        }

        public T GetComponentInChildren<T>()
        {
            return agent != null ? agent.GetComponentInChildren<T>() : default;
        }
    }
}