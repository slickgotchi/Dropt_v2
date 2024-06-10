namespace CarlosLab.UtilityIntelligence
{
    public class OtherFilter : TargetFilter
    {
        protected override bool OnFilterTarget(UtilityEntity target)
        {
            return target != this.Agent;
        }
    }
}