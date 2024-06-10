#region

using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputView : BaseView<ConsiderationViewModel>
    {
        private readonly TextField inputField;

        private readonly InputValueView inputValueView;
        private readonly NormalizationView normalizationView;

        private readonly TextField targetNameField;

        public InputView() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Input";
            Add(foldout);

            inputField = new TextField("Name");
            inputField.SetEnabled(false);
            foldout.Add(inputField);

            inputValueView = new InputValueView();
            foldout.Add(inputValueView);

            targetNameField = new TextField("Target");
            targetNameField.SetEnabled(false);
            foldout.Add(targetNameField);

            normalizationView = new NormalizationView();
            normalizationView.style.marginTop = 5;
            Add(normalizationView);
        }

        protected override void OnUpdateView(ConsiderationViewModel viewModel)
        {
            inputField.SetDataBinding(nameof(TextField.value), nameof(ConsiderationViewModel.InputName),
                BindingMode.ToTarget);

            inputValueView.UpdateView(viewModel);
            targetNameField.dataSource = viewModel.ContextViewModel;
            targetNameField.SetDataBinding(nameof(TextField.value),
                nameof(ConsiderationRuntimeContextViewModel.TargetName), BindingMode.ToTarget);

            normalizationView.UpdateView(viewModel);
        }

        protected override void OnRegisterViewModelEvents(ConsiderationViewModel viewModel)
        {
            viewModel.EditorViewModel.InputChanged += OnInputChanged;
        }

        protected override void OnUnregisterViewModelEvents(ConsiderationViewModel viewModel)
        {
            viewModel.EditorViewModel.InputChanged -= OnInputChanged;
        }

        private void OnInputChanged(InputItemViewModel newInput)
        {
            if (newInput != null)
                inputValueView.UpdateValueField(newInput.ValueType);
            else
                inputValueView.Clear();
        }
    }
}