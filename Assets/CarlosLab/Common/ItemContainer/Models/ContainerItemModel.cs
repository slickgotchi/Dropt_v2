#region

using System.Runtime.Serialization;

#endregion

namespace CarlosLab.Common
{
    [DataContract]
    public abstract class ContainerItemModel<TRuntime> : ModelWithId<TRuntime>, IContainerItem
        where TRuntime : IContainerItem
    {
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