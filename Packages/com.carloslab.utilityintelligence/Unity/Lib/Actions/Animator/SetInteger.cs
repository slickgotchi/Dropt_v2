using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    [Category("Animator")]
    public class SetInteger : SetParam
    {
        public VariableReference<int> Value;

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            animator.SetInteger(ParamName, Value);
            return UpdateStatus.Success;
        }
    }
}