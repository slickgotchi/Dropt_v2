using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public class TargetFilterReference : ItemReference<TargetFilter, TargetFilterContainer>
    {
        public TargetFilterReference(string name, TargetFilterContainer container = null) : base(name, container)
        {
        }
    }
}