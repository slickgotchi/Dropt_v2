#region

using System;
using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public abstract class InputNormalization : UtilityIntelligenceMember, IModel
    {
        private float normalizedInput;

        public float NormalizedInput
        {
            get => normalizedInput;

            protected set => normalizedInput = value;
        }

        public abstract Type ValueType { get; }

        internal abstract float CalculateNormalizedInput(ref InputContext context);
    }

    public abstract class InputNormalization<T> : InputNormalization
    {
        public override Type ValueType => typeof(T);

        internal override float CalculateNormalizedInput(ref InputContext context)
        {
            float normalizedInput = 0.0f;
            if (context.Input is Input<T> genericInput)
            {
                T rawInput = genericInput.GetRawInput(ref context);
                normalizedInput = CalculateNormalizedInput(rawInput, context);
            }

            NormalizedInput = normalizedInput;

            context.NormalizedInput = normalizedInput;
            return normalizedInput;
        }

        private float CalculateNormalizedInput(T rawInput, InputContext context)
        {
            float normalizedInput = OnCalculateNormalizedInput(rawInput, context);
            normalizedInput = Math.Clamp(normalizedInput, 0.0f, 1.0f);
            return normalizedInput;
        }

        protected abstract float OnCalculateNormalizedInput(T rawInput, InputContext context);
    }
}