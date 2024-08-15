using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    [Category("Animator")]
    public class SetBool : SetParam
    {
        public VariableReference<bool> Value;

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            animator.SetBool(ParamName, Value);
            return UpdateStatus.Success;
        }
    }
}