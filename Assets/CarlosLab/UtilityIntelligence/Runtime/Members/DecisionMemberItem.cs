using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public class DecisionMemberItem : DecisionMember, IContainerItem
    {
        #region IContainerItem

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