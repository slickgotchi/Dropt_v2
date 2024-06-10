#region

using System;
using System.Collections.Generic;
using System.Reflection;
using CarlosLab.Common;
using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterListViewModel : NameListViewModel<DecisionModel, TargetFilterModel, TargetFilterViewModel>
    {

        private TargetFilterEditorListViewModel editorViewModel;

        
        public TargetFilterListViewModel(IDataAsset asset, DecisionModel model) : base(asset, model)
        {
        }

        public override IReadOnlyList<TargetFilterModel> ItemModels => Model.TargetFilters;
        
        private TargetFilterEditorListViewModel EditorViewModel
        {
            get
            {
                if (editorViewModel == null)
                    EditorViewModel = UtilityIntelligenceEditorUtils.TargetFilters;

                return editorViewModel;
            }
            set
            {
                if (editorViewModel == value)
                    return;

                if (editorViewModel != null) UnregisterEditortViewModelEvents(editorViewModel);
                editorViewModel = value;
                if (editorViewModel != null) RegisterEditorViewModelEvents(editorViewModel);
            }
        }
        
        protected override TargetFilterViewModel CreateViewModel(TargetFilterModel model)
        {
            TargetFilterViewModel itemViewModel = base.CreateViewModel(model);
            TargetFilterEditorViewModel tabItem = EditorViewModel.GetItemByName(model.Name);
            itemViewModel.EditorViewModel = tabItem;
            return itemViewModel;
        }

        public override bool Contains(string name)
        {
            return Model.HasTargetFilter(name);
        }

        protected override bool TryAddModelWithoutRecord(TargetFilterModel model, int index)
        {
            Model.TryAddTargetFilter(index, model.Name, model);
            return true;
        }

        protected override bool TryRemoveModelWithoutRecord(TargetFilterModel model)
        {
            Model.TryRemoveTargetFilter(model.Name, model);
            return true;
        }
        
        protected override bool TryRemoveModelWithoutRecord(string name, TargetFilterModel model)
        {
            return Model.TryRemoveTargetFilter(name, model);
        }
        
        protected override void OnItemIndexChanged(int sourceIndex, int destIndex)
        {
            Model.MoveTargetFilter(sourceIndex, destIndex);
        }
        
        private void UnregisterEditortViewModelEvents(TargetFilterEditorListViewModel viewModel)
        {
            viewModel.ItemRemoved -= OnTargetFilterRemoved;
            viewModel.ItemNameChanged -= OnTargetFilterNameChanged;
        }

        private void RegisterEditorViewModelEvents(TargetFilterEditorListViewModel viewModel)
        {
            viewModel.ItemRemoved += OnTargetFilterRemoved;
            viewModel.ItemNameChanged += OnTargetFilterNameChanged;
        }

        private void OnTargetFilterRemoved(TargetFilterEditorViewModel item)
        {
            TryRemoveItem(item.Name);
        }

        private void OnTargetFilterNameChanged(TargetFilterEditorViewModel item, string oldName, string newName)
        {
            Model.OnTargetFilterNameChanged(oldName, newName);
        }
    }
}