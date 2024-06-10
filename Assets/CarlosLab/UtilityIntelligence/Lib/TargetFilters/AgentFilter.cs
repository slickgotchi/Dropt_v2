namespace CarlosLab.UtilityIntelligence
{
    public class AgentFilter : TargetFilter
    {
        protected override bool OnFilterTarget(UtilityEntity target)
        {
            return target is UtilityAgent;
        }
    }
}