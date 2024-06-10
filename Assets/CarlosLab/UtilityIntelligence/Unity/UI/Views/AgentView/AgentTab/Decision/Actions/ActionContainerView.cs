#region

using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class ActionContainerView : BaseView<DecisionViewModel>, IMainView<DecisionSubView>
    {
        private readonly EnumField executionModeField;
        private readonly ActionItemCreatorView itemCreatorView;
        private readonly ActionListView listView;

        private readonly Toggle keepRunningUntilFinishedToggle;
        private readonly IntegerField maxRepeatCountField;
        private readonly IntegerField currentRepeatCountField;

        private readonly Toggle reorderableToggle;

        public ActionContainerView() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Actions";
            Add(foldout);

            keepRunningUntilFinishedToggle = new("Keep Running Until Finished");
            keepRunningUntilFinishedToggle.tooltip = ToolTips.KeepRunningUntilFinished;
            keepRunningUntilFinishedToggle.RegisterValueChangedCallback(evt => ViewModel.KeepRunningUntilFinished = evt.newValue);
            foldout.Add(keepRunningUntilFinishedToggle);
            
            maxRepeatCountField = new("Max Repeat Count");
            maxRepeatCountField.isDelayed = true;
            maxRepeatCountField.tooltip = ToolTips.MaxRepeatCount;
            maxRepeatCountField.RegisterValueChangedCallback(evt =>
            {
                int maxRepeatCount = evt.newValue;
                ViewModel.MaxRepeatCount = maxRepeatCount;
                UpdateCurrentRepeatCountFieldDisplay();
            });
            foldout.Add(maxRepeatCountField);
            
            currentRepeatCountField = new("Current Repeat Count");
            currentRepeatCountField.SetEnabled(false);
            currentRepeatCountField.tooltip = ToolTips.CurrentRepeatCount;
            foldout.Add(currentRepeatCountField);
            
            reorderableToggle = new("Reorderable");
            reorderableToggle.tooltip = ToolTips.Reorderable;
            reorderableToggle.style.borderTopColor = new Color(0.5f, 0.5f, 0.5f);
            reorderableToggle.style.borderTopWidth = 1.0f;
            reorderableToggle.style.paddingTop = 5;
            reorderableToggle.RegisterValueChangedCallback(evt => listView.Reorderable = evt.newValue);
            foldout.Add(reorderableToggle);

            listView = new ActionListView();
            listView.style.marginTop = 5;
            foldout.Add(listView);

            executionModeField = new EnumField(ActionExecutionMode.Sequence);
            executionModeField.style.marginTop = 10;
            executionModeField.RegisterValueChangedCallback(evt =>
            {
                ViewModel.ActionExecutionMode = (ActionExecutionMode)evt.newValue;
            });

            foldout.Add(executionModeField);

            itemCreatorView = new ActionItemCreatorView();
            foldout.Add(itemCreatorView);
        }

        public DecisionSubView SubView { get; private set; }

        public void InitSubView(DecisionSubView subView)
        {
            SubView = subView;
            listView.InitSubView(subView);
            listView.ItemAdded += item => { UpdateExecutionModeFieldDisplay(); };

            listView.ItemRemoved += item => { UpdateExecutionModeFieldDisplay(); };
        }

        protected override void OnUpdateView(DecisionViewModel viewModel)
        {
            listView.UpdateView(viewModel.Actions);
            itemCreatorView.UpdateView(viewModel.Actions);
            
            keepRunningUntilFinishedToggle.SetDataBinding(
            nameof(Toggle.value), 
            nameof(DecisionViewModel.KeepRunningUntilFinished), 
            BindingMode.ToTarget);

            maxRepeatCountField.SetDataBinding(
                nameof(IntegerField.value), 
                nameof(DecisionViewModel.MaxRepeatCount), 
                BindingMode.ToTarget);


            UpdateCurrentRepeatCountField();

            UpdateExecutionModeField();
        }

        private void UpdateCurrentRepeatCountField()
        {
            currentRepeatCountField.value = ViewModel.CurrentRepeatCount;
            currentRepeatCountField.SetDataBinding(nameof(IntegerField.value), nameof(DecisionViewModel.CurrentRepeatCount),
                BindingMode.ToTarget);

            UpdateCurrentRepeatCountFieldDisplay();
        }

        private void UpdateCurrentRepeatCountFieldDisplay()
        {
            if(maxRepeatCountField.value > 0)
                currentRepeatCountField.SetDisplay(true);
            else
                currentRepeatCountField.SetDisplay(false);
        }

        private void UpdateExecutionModeField()
        {
            executionModeField.SetValueWithoutNotify(ViewModel.ActionExecutionMode);
            UpdateExecutionModeFieldDisplay();
        }
        
        private void UpdateExecutionModeFieldDisplay()
        {
            if (ViewModel.Actions.Items.Count >= 2)
                executionModeField.SetDisplay(true);
            else
                executionModeField.SetDisplay(false);
        }

    }
}