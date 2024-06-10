#region

using System.Runtime.Serialization;
using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public class UtilityIntelligenceMemberItem : UtilityIntelligenceMember, IModelWithId, IContainerItem
    {
        #region IModelWithId

        [DataMember(Name = nameof(Id))]
        private string id;
        public string Id => id;
        
        string IModelWithId.Id
        {
            get => id;
            set => id = value;
        }

        #endregion
        
        #region IContainerItem

        [DataMember(Name = nameof(Name))]
        private string name;

        public string Name
        {
            get => name;
            internal set => name = value;
        }

        private bool isInContainer;
        public bool IsInContainer => isInContainer;

        string IContainerItem.Name
        {
            get => name;
            set => name = value;
        }

        bool IContainerItem.IsInContainer
        {
            get => isInContainer;
            set => isInContainer = value;
        }

        #endregion
    }
}