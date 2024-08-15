using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    [Category("Animator")]
    public class SetFloat : SetParam
    {
        public VariableReference<float> Value;

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            animator.SetFloat(ParamName, Value);
            return UpdateStatus.Success;
        }
    }
}