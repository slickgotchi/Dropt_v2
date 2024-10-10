using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CanvasFadeIn : MonoBehaviour
{
    public CanvasGroup canvasGroup; // Reference to the CanvasGroup
    public float fadeDuration = 1f; // Duration of the fade-in effect

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

        // Fade the alpha from 0 to 1 over the specified duration
        canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.InOutQuad);
    }
}
