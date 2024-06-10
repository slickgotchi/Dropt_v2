#region

using System;
using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public abstract class Input : UtilityIntelligenceMemberItem, IContainerItemValue
    {
        #region Value

        public event Action ValueChanged;
        public abstract object ValueObject { get; internal set; }
        
        public abstract Type ValueType { get; }

        protected void RaiseValueChanged()
        {
            ValueChanged?.Invoke();
        }

        #endregion
    }

    public abstract class Input<TValue> : Input, IContainerItemValue<TValue>
    {
        #region Value

        private TValue value;

        public TValue Value
        {
            get => value;
            internal set
            {
                this.value = value;
                RaiseValueChanged();
            }
        }

        public override object ValueObject
        {
            get => value;
            internal set
            {
                if (value is TValue tValue)
                    this.value = tValue;
            }
        }
        
        public override Type ValueType => typeof(TValue);

        #endregion

        #region GetRawInput

        internal TValue GetRawInput(ref InputContext context)
        {
            if (Intelligence.IsRuntimeAsset)
            {
                Value = OnGetRawInput(context);
                
                if(Intelligence.IsEditorOpening)
                    context.RawInput = value;
            }

            return value;
        }

        protected virtual TValue OnGetRawInput(InputContext context)
        {
            return value;
        }

        #endregion
    }
}