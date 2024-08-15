using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public interface INoTargetItem : IContainerItem
    {
        public bool HasNoTarget { get; }
    }
}