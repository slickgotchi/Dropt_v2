using UnityEngine;
using DG.Tweening;

public class CanvasFadeIn : MonoBehaviour
{
    public CanvasGroup canvasGroup; // Reference to the CanvasGroup
    public float fadeDuration = 1f; // Duration of the fade-in effect
    private Tweener fadeTween;

    private void Start()
    {
        if (canvasGroup == null) return;

        // Start with the canvas invisible
        canvasGroup.alpha = 0f;

        // Fade in the canvas
        FadeInCanvas();
    }

    // Fade in the canvas group
    public void FadeInCanvas()
    {
        if (canvasGroup == null) return;

        // Kill any existing tweens on the canvasGroup to prevent conflicts
        canvasGroup.DOKill();

        // Fade the alpha from 0 to 1 over the specified duration and store the tween reference
        fadeTween = canvasGroup.DOFade(1f, fadeDuration)
                              .SetEase(Ease.InOutQuad);
    }

    private void OnDestroy()
    {
        // Kill the tween to prevent it from running after the object is destroyed
        if (fadeTween != null && fadeTween.IsActive())
        {
            fadeTween.Kill();
        }
    }
}
