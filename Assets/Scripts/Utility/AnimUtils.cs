using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Dropt.Utils
{
    public static class Anim
    {
        public static async void PlayAnimationWithDuration(Animator animator, string animName, float duration, float delaySeconds = 0)
        {
            if (animator == null)
            {
                Debug.LogWarning("No animator for gameObject trying to play " + animName);
                return;
            }

            if (delaySeconds > 0 && !Bootstrap.IsHost())
            {
                await UniTask.Delay((int)(delaySeconds * 1000));
            }

            // Get the animation clip
            AnimationClip clip = null;
            foreach (var ac in animator.runtimeAnimatorController.animationClips)
            {
                if (ac.name == animName) clip = ac;
            }

            if (clip == null)
            {
                Debug.LogWarning($"Animation clip '{animName}' not found in animator.");
                return;
            }

            // Calculate adjusted speed
            float speedMult = clip.length / duration;

            // Set animator speed
            animator.SetFloat("AnimationSpeed", speedMult);

            // Play the animation
            animator.Play(animName);
        }

        public static async void Play(Animator animator, string animName, float delaySeconds = 0)
        {
            if (animator == null)
            {
                Debug.LogWarning("No animator for gameObject trying to play " + animName);
                return;
            }

            if (delaySeconds > 0 && !Bootstrap.IsHost())
            {
                await UniTask.Delay((int)(delaySeconds * 1000));
            }

            // Play the animation
            animator.Play(animName);
        }
    }
}
