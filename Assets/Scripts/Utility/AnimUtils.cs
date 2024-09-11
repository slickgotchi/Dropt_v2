using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dropt.Utils
{

    public static class Anim
    {
        public static void PlayAnimationWithDuration(Animator animator, string animName, float duration)
        {
            // get our anim clip
            AnimationClip clip = null;
            foreach (var ac in animator.runtimeAnimatorController.animationClips)
            {
                if (ac.name == animName) clip = ac;
            }
            if (clip == null) return;

            // calc adjusted speed
            float speedMult = clip.length / duration;

            // set animator speed
            animator.SetFloat("AnimationSpeed", speedMult);

            // play the animation
            animator.Play(animName);
        }
    }
}
