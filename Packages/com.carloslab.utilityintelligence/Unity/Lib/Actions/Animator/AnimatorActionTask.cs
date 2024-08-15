using CarlosLab.Common;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class AnimatorActionTask : ActionTask
    {
        public VariableReference<Animator> Animator;

        protected Animator animator => Animator.Value;
        
        protected override void OnAwake()
        {
            if (Animator.Value == null)
                Animator.Value = GetComponentInChildren<Animator>();
        }
    }
}