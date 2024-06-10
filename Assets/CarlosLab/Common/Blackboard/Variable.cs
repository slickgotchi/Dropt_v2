﻿#region

using System;
using System.Runtime.Serialization;
using CarlosLab.Common;

#endregion

namespace CarlosLab.Common
{
    public abstract class Variable : ContainerItem, IContainerItemValue
    {
        public abstract object ValueObject { get; set; }

        public abstract Type ValueType { get; }
    }

    public abstract class Variable<TValueType> : Variable, IContainerItemValue<TValueType>
    {
        [DataMember(Name = nameof(Value))]
        private TValueType value;

        public Variable()
        {
        }

        public Variable(TValueType value)
        {
            this.value = value;
        }

        #region Properties

        public TValueType Value
        {
            get => value;
            set => this.value = value;
        }

        public override object ValueObject
        {
            get => Value;
            set
            {
                if (value is TValueType tValue)
                    this.value = tValue;
            }
        }

        public override Type ValueType => typeof(TValueType);

        #endregion
    }
}