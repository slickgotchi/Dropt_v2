
namespace CarlosLab.UtilityIntelligence
{
    [Category("Animator")]
    public class SetTrigger : SetParam
    {
        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            animator.SetTrigger(ParamName);
            return UpdateStatus.Success;
        }
    }
}