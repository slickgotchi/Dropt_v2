using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public interface IUtilityIntelligenceComponent
    {
        public Blackboard Blackboard { get; }
        UtilityAgent Agent { get; }
        IEntityFacade AgentFacade { get; }
        
        T GetComponent<T>();
        T GetComponentInChildren<T>();
    }
}