﻿#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class VariableViewModel : BaseItemViewModel<Variable, BlackboardViewModel>, INameViewModel, IValueViewModel
        , INotifyBindablePropertyChanged
    {
        
        public event Action ValueChanged;

        [CreateProperty]
        public string Name
        {
            get => Model?.Name ?? UtilityIntelligenceUIConsts.DefaultItemName;
            set
            {
                if (Model == null || Model.Name == value)
                    return;
                
                Record($"VariableViewModel Name Changed: {value}",
                    () => { Model.Name = value; });
                    
                Notify();
            }
        }

        [CreateProperty]
        public object ValueObject
        {
            get => Model?.ValueObject;
            set
            {
                if (Model == null || Model.ValueObject == value)
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
            RootViewModel.MakeDecision();
        }
    }
}