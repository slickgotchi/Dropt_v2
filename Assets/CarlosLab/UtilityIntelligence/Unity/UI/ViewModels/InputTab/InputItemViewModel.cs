#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class InputItemViewModel : BaseItemViewModel<InputModel>, ITypeNameViewModel, INameViewModel, IValueViewModel
        , INotifyBindablePropertyChanged

    {
        private object valueObject;

        public InputItemViewModel(IDataAsset asset, InputModel model) : base(asset, model)
        {
        }

        public string TypeName => Model.RuntimeType.Name;

        public Type ValueType => Model.ValueType;

        public event Action ValueChanged;

        private void NotifyValueChanged()
        {
            Notify(nameof(ValueObject));
        }

        protected override void OnModelChanged(InputModel newModel)
        {
            Notify(nameof(Name));
            if (Asset.IsInUndoRedo)
            {
                ValueObject = valueObject;
                NotifyValueChanged();
            }
        }

        protected override void RegisterModelEvents(InputModel model)
        {
            model.Runtime.ValueChanged += NotifyValueChanged;
        }

        protected override void UnregisterModelEvents(InputModel model)
        {
            model.Runtime.ValueChanged -= NotifyValueChanged;
        }

        #region Binding Properties

        [CreateProperty]
        public string Name
        {
            get => Model.Name;
            set
            {
                if (Model.Name == value) return;

                Record($"InputTabItemViewModel Name Changed: {value}",
                    () => { Model.Name = value; });

                Notify();
            }
        }

        [CreateProperty]
        public object ValueObject
        {
            get => Model.Runtime.ValueObject;
            set
            {
                if (Model.Runtime.ValueObject == value) return;

                Model.Runtime.ValueObject = value;
                valueObject = value;
                OnValueChanged();
            }
        }

        #endregion

        private void OnValueChanged()
        {
            NotifyValueChanged();
            ValueChanged?.Invoke();
        }
    }
}