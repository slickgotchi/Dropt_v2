#region

using CarlosLab.Common;
using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterViewModel : BaseItemViewModel<TargetFilterModel>, INameViewModel, INotifyBindablePropertyChanged
    {
        private TargetFilterEditorViewModel editorViewModel;

        public TargetFilterViewModel(IDataAsset asset, TargetFilterModel model) : base(asset, model)
        {
        }

        [CreateProperty]
        public string Name
        {
            get => EditorViewModel.Name;
            set => EditorViewModel.Name = value;
        }
        
        public TargetFilterEditorViewModel EditorViewModel
        {
            get => editorViewModel;
            set
            {
                if (editorViewModel == value)
                    return;

                if (editorViewModel != null)
                    UnregisterEditorViewModelEvents(editorViewModel);

                editorViewModel = value;

                if (editorViewModel != null)
                    RegisterEditorViewModelEvents(editorViewModel);
            }
        }
        
        private void RegisterEditorViewModelEvents(TargetFilterEditorViewModel viewModel)
        {
            viewModel.propertyChanged += OnEditorViewModelPropertyChanged;
        }

        private void UnregisterEditorViewModelEvents(TargetFilterEditorViewModel viewModel)
        {
            viewModel.propertyChanged -= OnEditorViewModelPropertyChanged;
        }
        
        private void OnEditorViewModelPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            Notify(e.propertyName);
        }
    }
}