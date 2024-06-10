#region

using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public class UtilityEntity : Entity<UtilityEntity, UtilityWorld>
    {
        public IImapEntity ImapEntity { get; internal set; }
    }
}