using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Dropt.Utils
{
    public static class Anim
    {
        public static async void PlayAnimationWithDuration(Animator animator, string animName, float duration, float delaySeconds = 0)
        {
            // Check if animator is valid
            if (animator == null || animator.gameObject == null)
            {
                Debug.LogWarning("No valid animator for gameObject trying to play " + animName);
                return;
            }

            if (delaySeconds > 0 && !Bootstrap.IsHost())
            {
                await UniTask.Delay((int)(delaySeconds * 1000));
            }

            // Check again after delay (in case the animator was destroyed)
            if (animator == null || animator.gameObject == null)
            {
                Debug.LogWarning("Animator was destroyed before animation could be played.");
                return;
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
            // Check if animator is valid
            if (animator == null || animator.gameObject == null)
            {
                //Debug.LogWarning("No valid animator for gameObject trying to play " + animName);
                return;
            }

            if (delaySeconds > 0 && !Bootstrap.IsHost())
            {
                await UniTask.Delay((int)(delaySeconds * 1000));
            }

            // Check again after delay (in case the animator was destroyed)
            if (animator == null || animator.gameObject == null)
            {
                //Debug.LogWarning("Animator was destroyed before animation could be played.");
                return;
            }

            // Play the animation
            animator.Play(animName);
        }
    }
}
