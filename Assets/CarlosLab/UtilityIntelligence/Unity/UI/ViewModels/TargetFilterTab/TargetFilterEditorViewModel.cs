using CarlosLab.Common;
using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterEditorViewModel : BaseItemViewModel<TargetFilterModel, TargetFilterEditorListViewModel>,
        INameViewModel, ITypeNameViewModel, INotifyBindablePropertyChanged
    {
        public TargetFilterEditorViewModel(IDataAsset asset, TargetFilterModel model) : base(asset, model)
        {
        }
        
        public string TypeName => Model.RuntimeType.Name;

        [CreateProperty]
        public string Name
        {
            get => Model.Name;
            set
            {
                if (Model.Name == value)
                    return;

                Record($"TargetFilterEditorViewModel Name Changed: {value}",
                    () => { Model.Name = value; });

                Notify();
            }
        }
        
        protected override void OnModelChanged(TargetFilterModel newModel)
        {
            Notify(nameof(Name));
        }
    }
}