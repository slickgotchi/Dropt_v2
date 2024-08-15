using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public interface IUtilityIntelligenceMember : IRootObjectMember<UtilityIntelligence>, IUtilityIntelligenceComponent
    {
        UtilityIntelligence Intelligence { get; }
    }
}