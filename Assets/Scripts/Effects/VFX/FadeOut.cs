using UnityEngine;

public class FadeOut : MonoBehaviour
{
    public float duration = 1.0f;  // Duration of the fade out
    public float startAlpha = 1.0f;  // Starting alpha value
    public float finalAlpha = 0.0f;  // Final alpha value

    private SpriteRenderer[] spriteRenderers;
    private float elapsedTime = 0.0f;
    private bool isFading = true;

    void Start()
    {
        // Get all SpriteRenderer components in children
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        // Set initial alpha
        SetAlpha(startAlpha);
    }

    void Update()
    {
        if (isFading)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, finalAlpha, elapsedTime / duration);

            // Set the alpha for all sprite renderers
            SetAlpha(alpha);

            // Check if the fade out is complete
            if (elapsedTime >= duration)
            {
                isFading = false;
                SetAlpha(finalAlpha);
            }
        }
    }

    private void SetAlpha(float alpha)
    {
        foreach (var spriteRenderer in spriteRenderers)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}
