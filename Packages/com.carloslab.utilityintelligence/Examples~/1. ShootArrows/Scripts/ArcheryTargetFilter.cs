
namespace CarlosLab.UtilityIntelligence.Examples
{
    public class ArcheryTargetFilter : TargetFilter
    {
        protected override bool OnFilterTarget(UtilityEntity target)
        {
            return target.EntityFacade is ArcheryTarget;
        }
    }
}
