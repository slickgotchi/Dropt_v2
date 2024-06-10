#region

using System;
using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public sealed class Consideration : UtilityIntelligenceMemberItem
    {
        public Consideration(Input input, InputNormalization inputNormalization, InfluenceCurve responseCurve)
        {
            this.input = input;
            this.inputNormalization = inputNormalization;
            this.responseCurve = responseCurve;
        }
        
        #region Status

        private Status currentStatus;
        
        public Status CurrentStatus
        {
            get => currentStatus;
            internal set => currentStatus = value;
        }

        #endregion

        #region Input
        
        private Input input;
        public Input Input
        {
            get => input;
            internal set
            {
                if (input == value)
                    return;

                input = value;

                if (input != null)
                    input.Intelligence = Intelligence;
            }
        }
        
        public object RawInput => Input?.ValueObject;
        
        #endregion

        #region InputNormalization

        private InputNormalization inputNormalization;
        
        public float NormalizedInput => inputNormalization?.NormalizedInput ?? 0f;
        public InputNormalization InputNormalization
        {
            get => inputNormalization;
            internal set
            {
                if (inputNormalization == value) return;

                inputNormalization = value;

                OnInputNormalizationChanged();
            }
        }
        
        private void OnInputNormalizationChanged()
        {
            if (inputNormalization != null) inputNormalization.Intelligence = Intelligence;
        }

        #endregion
        
        #region Consideration
        
        public bool HasNoTarget { get; internal set; }
        
        public ConsiderationContext NoTargetContext { get; internal set; }
        
        private InfluenceCurve responseCurve;
        public InfluenceCurve ResponseCurve
        {
            get => responseCurve;
            internal set => responseCurve = value;
        }
        
        private float score;
        
        public float Score
        {
            get => score;
            internal set
            {
                score = value;
                ScoreChanged?.Invoke();
            }
        }
        public event Action ScoreChanged;

        
        public float CalculateScore(ref ConsiderationContext context)
        {
            using var _ = Profiler.Sample("CalculateScore - Consideration");

            InputContext inputContext = new(input, context.Target);
            float normalizedInput = InputNormalization?.CalculateNormalizedInput(ref inputContext) ?? 0f;
            float score = InfluenceCurveUtils.Evaluate(in normalizedInput, in responseCurve);
            Score = score;
            context.Score = score;
            context.Input = inputContext;
            return score;
        }

        protected override void OnIntelligenceChanged(UtilityIntelligence intelligence)
        {
            if (inputNormalization != null) inputNormalization.Intelligence = intelligence;
        }

        #endregion

    }
}