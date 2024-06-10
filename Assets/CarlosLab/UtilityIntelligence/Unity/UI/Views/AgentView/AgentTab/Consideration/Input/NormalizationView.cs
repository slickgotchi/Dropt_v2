#region

using CarlosLab.Common;
using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class NormalizationView : BaseView<ConsiderationViewModel>
    {
        private readonly TextField nameField;
        private readonly FloatField normalizedInputField;
        private readonly TextField targetNameField;

        public NormalizationView() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Input Normalization";
            Add(foldout);

            nameField = new TextField("Type");
            nameField.SetEnabled(false);
            foldout.Add(nameField);

            normalizedInputField = new FloatField("Normalized Input");
            normalizedInputField.SetEnabled(false);
            foldout.Add(normalizedInputField);

            targetNameField = new TextField("Target");
            targetNameField.SetEnabled(false);
            foldout.Add(targetNameField);
        }

        protected override void OnUpdateView(ConsiderationViewModel viewModel)
        {
            nameField.SetDataBinding(nameof(TextField.value), nameof(ConsiderationViewModel.NormalizationName),
                BindingMode.ToTarget);

            if (ViewModel.Asset.IsRuntimeAsset)
            {
                normalizedInputField.dataSource = viewModel.ContextViewModel;
                normalizedInputField.SetDataBinding(nameof(FloatField.value),
                    nameof(ConsiderationRuntimeContextViewModel.NormalizedInput), BindingMode.ToTarget);
            }
            else
            {
                normalizedInputField.dataSource = viewModel.RuntimeViewModel;
                normalizedInputField.SetDataBinding(nameof(FloatField.value),
                    nameof(ConsiderationRuntimeViewModel.NormalizedInput), BindingMode.ToTarget);
            }

            UpdateNormalizationInputDisplay();

            targetNameField.dataSource = viewModel.ContextViewModel;
            targetNameField.SetDataBinding(nameof(TextField.value),
                nameof(ConsiderationRuntimeContextViewModel.TargetName), BindingMode.ToTarget);
        }

        protected override void OnRegisterViewModelEvents(ConsiderationViewModel viewModel)
        {
            viewModel.EditorViewModel.NormalizationCache.NormalizationChanged += OnNormalizationChanged;
        }

        protected override void OnUnregisterViewModelEvents(ConsiderationViewModel viewModel)
        {
            viewModel.EditorViewModel.NormalizationCache.NormalizationChanged -= OnNormalizationChanged;
        }

        private void OnNormalizationChanged(NormalizationViewModel viewModel)
        {
            UpdateNormalizationInputDisplay();
        }

        private void UpdateNormalizationInputDisplay()
        {
            if (ViewModel.Normalization != null)
                normalizedInputField.SetDisplay(true);
            else
                normalizedInputField.SetDisplay(false);
        }
    }
}