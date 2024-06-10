#region

using System.Runtime.Serialization;
using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    [DataContract]
    public class ConsiderationModel : ContainerItemModel<Consideration>
    {
        #region Input

        public InputContainerModel InputContainer { get; internal set; }

        [DataMember(Name = nameof(Input))]
        private string inputName;

        public string InputName
        {
            get => inputName;
            set
            {
                if (inputName == value) return;

                inputName = value;
                if (InputContainer != null && InputContainer.TryGetItem(inputName, out InputModel input)) Input = input;
            }
        }
        
        private InputModel input;

        private InputModel Input
        {
            get
            {
                if (input == null)
                    Input = InputContainer?.GetItemByName(inputName);

                return input;
            }
            set
            {
                if (input == value) return;

                input = value;
                Runtime.Input = value.Runtime;
            }
        }

        #endregion

        #region InputNormalization

        [DataMember(Name = nameof(InputNormalization))]
        private InputNormalizationModel inputNormalization;

        public InputNormalizationModel InputNormalization
        {
            get => inputNormalization;
            set
            {
                if (inputNormalization == value) return;

                inputNormalization = value;
                Runtime.InputNormalization = inputNormalization?.Runtime;
            }
        }

        #endregion

        #region Consideration
        
        [DataMember(Name = nameof(HasNoTarget))]
        private bool hasNoTarget;
        public bool HasNoTarget
        {
            get => hasNoTarget;
            set
            {
                hasNoTarget = value;
                Runtime.HasNoTarget = value;
            }
        }

        [DataMember(Name = nameof(ResponseCurve))]
        private InfluenceCurve responseCurve = InfluenceCurve.BasicLinear;

        public InfluenceCurve ResponseCurve
        {
            get => responseCurve;
            set
            {
                responseCurve = value;
                Runtime.ResponseCurve = value;
            }
        }

        #endregion

        #region Runtime

        private Consideration runtime;

        public override Consideration Runtime
        {
            get
            {
                Input input = Input?.Runtime;
                InputNormalization inputNormalization = InputNormalization?.Runtime;
                if (runtime == null)
                {
                    runtime = new Consideration(input, inputNormalization, ResponseCurve)
                    {
                        HasNoTarget = hasNoTarget
                    };
                }

                return runtime;
            }
        }

        #endregion
        
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (context.Context is UtilityIntelligenceAsset asset)
            {
                asset.Considerations.Add(this);
            }
        }
    }
}