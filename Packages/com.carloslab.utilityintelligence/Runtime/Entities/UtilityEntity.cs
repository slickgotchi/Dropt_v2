#region

using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public class UtilityEntity : Entity<UtilityEntity, UtilityWorld>
    {
        public static readonly UtilityEntity Null = new();
        
        public IImapEntity ImapEntity { get; internal set; }
    }
}