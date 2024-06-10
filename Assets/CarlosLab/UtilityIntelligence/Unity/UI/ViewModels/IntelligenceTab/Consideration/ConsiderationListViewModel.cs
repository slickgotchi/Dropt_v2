#region

using System;
using System.Collections.Generic;
using CarlosLab.Common;
using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class
        ConsiderationListViewModel : NameListViewModel<DecisionModel, ConsiderationModel, ConsiderationViewModel>
    {
        private ConsiderationEditorListViewModel editorViewModel;

        public DecisionContext Context => Model?.Runtime.Context ?? default;

        public event Action<DecisionContext> ContextChanged;

        public ConsiderationListViewModel(IDataAsset asset, DecisionModel model) : base(asset, model)
        {
        }

        private ConsiderationEditorListViewModel EditorViewModel
        {
            get
            {
                if (editorViewModel == null)
                    EditorViewModel = UtilityIntelligenceEditorUtils.Considerations;

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

        public override IReadOnlyList<ConsiderationModel> ItemModels => Model.Considerations;

        public override bool Contains(string name)
        {
            return Model.HasConsideration(name);
        }

        protected override ConsiderationViewModel CreateViewModel(ConsiderationModel model)
        {
            ConsiderationViewModel itemViewModel = base.CreateViewModel(model);
            ConsiderationEditorViewModel tabItem = EditorViewModel.GetItemByName(model.Name);
            itemViewModel.EditorViewModel = tabItem;
            return itemViewModel;
        }

        protected override bool TryAddModelWithoutRecord(ConsiderationModel model, int index)
        {
            return Model.TryAddConsideration(index, model.Name, model);
        }

        protected override bool TryRemoveModelWithoutRecord(ConsiderationModel model)
        {
            return Model.TryRemoveConsideration(model.Name, model);
        }

        protected override bool TryRemoveModelWithoutRecord(string name, ConsiderationModel model)
        {
            return Model.TryRemoveConsideration(name, model);
        }

        protected override void OnItemIndexChanged(int sourceIndex, int destIndex)
        {
            Model.MoveConsideration(sourceIndex, destIndex);
        }

        private void UnregisterEditortViewModelEvents(ConsiderationEditorListViewModel viewModel)
        {
            viewModel.ItemRemoved -= OnItemRemoved;
            viewModel.ItemNameChanged -= OnItemNameChanged;
        }

        private void RegisterEditorViewModelEvents(ConsiderationEditorListViewModel viewModel)
        {
            viewModel.ItemNameChanged += OnItemNameChanged;
            viewModel.ItemRemoved += OnItemRemoved;
        }
        
        
        private void OnItemNameChanged(ConsiderationEditorViewModel item, string oldName, string newName)
        {
            Model.OnConsiderationNameChanged(oldName, newName);
        }

        private void OnItemRemoved(ConsiderationEditorViewModel consideration)
        {
            TryRemoveItem(consideration.Name);
        }

        protected override void OnRegisterModelEvents(DecisionModel model)
        {
            model.Runtime.ContextChanged += OnContextChanged;
        }

        protected override void OnUnregisterModelEvents(DecisionModel model)
        {
            model.Runtime.ContextChanged -= OnContextChanged;
        }

        private void OnContextChanged(DecisionContext newContext)
        {
            ContextChanged?.Invoke(newContext);
        }
    }
}