#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class VariableViewModel : BaseItemViewModel<Variable>, INameViewModel, IValueViewModel
        , INotifyBindablePropertyChanged
    {
        
        public event Action ValueChanged;

        public VariableViewModel(IDataAsset asset, Variable model) : base(asset, model)
        {
        }

        [CreateProperty]
        public string Name
        {
            get => Model.Name;
            set
            {
                if (Model.Name == value)
                    return;

                Record($"VariableViewModel Name Changed: {value}",
                    () => { Model.Name = value; });
                Notify();
            }
        }

        [CreateProperty]
        public object ValueObject
        {
            get => Model.ValueObject;
            set
            {
                if (Model.ValueObject == value)
                    return;

                Record($"VariableViewModel Value Changed: {value}",
                    () => { Model.ValueObject = value; });
                OnValueChanged();
            }
        }

        public Type ValueType => Model.ValueType;

        protected override void OnModelChanged(Variable newModel)
        {
            Notify(nameof(Name));
            NotifyValueChanged();
        }
        
        private void NotifyValueChanged()
        {
            Notify(nameof(ValueObject));
        }

        private void OnValueChanged()
        {
            NotifyValueChanged();
            ValueChanged?.Invoke();
            
            MakeDecision();
        }
        
        private void MakeDecision()
        {
            UtilityIntelligenceEditorUtils.Model?.Runtime.MakeDecision(null);
        }
    }
}