using CarlosLab.Common;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence
{
    [Category("Animator")]
    public class WaitUntilAnimationFinished : AnimatorActionTask
    {
        public VariableReference<string> AnimationName;
        public float FinishedNormalizedTime = 0.75f;

        public bool IsAnimationFinished
        {
            get
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName(AnimationName) && stateInfo.normalizedTime >= FinishedNormalizedTime)
                    return true;

                return false;
            }
        }
        
        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            if (IsAnimationFinished)
                return UpdateStatus.Success;

            return UpdateStatus.Running;
        }
    }
}