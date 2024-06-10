#region

using System.Runtime.Serialization;

#endregion

namespace CarlosLab.Common
{
    [DataContract]
    public class ContainerItem : ModelWithId, IContainerItem
    {
        private bool isInContainer;

        [DataMember(Name = nameof(Name))]
        private string name;

        public string Name
        {
            get => name;
            internal set => name = value;
        }

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
    }
}