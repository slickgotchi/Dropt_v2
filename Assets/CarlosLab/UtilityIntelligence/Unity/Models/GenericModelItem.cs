using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public class GenericModelItem<TRuntime> : GenericModelWithId<TRuntime>, IContainerItem
        where TRuntime : class
    {
        #region IContainerItem

        public string Name
        {
            get => GetValue(nameof(Name)) as string;
            internal set => SetValue(nameof(Name), value);
        }
        
        string IContainerItem.Name
        {
            get => GetValue(nameof(IContainerItem.Name)) as string;
            set => SetValue(nameof(IContainerItem.Name), value);
        }

        private bool isInContainer;

        public bool IsInContainer => isInContainer;

        bool IContainerItem.IsInContainer
        {
            get => isInContainer;
            set => isInContainer = value;
        }

        #endregion
    }
}