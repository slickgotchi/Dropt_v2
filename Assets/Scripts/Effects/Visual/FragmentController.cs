using UnityEngine;

public class FragmentController : MonoBehaviour
{
    public float fadeOutDuration = 2f;
    public FadeOutType fadeOutType = FadeOutType.Linear; // Enum for fade-out type
    public int polynomialPower = 2; // Power for polynomial fade-out
    private SpriteRenderer spriteRenderer;
    private float fadeOutTimer = 0f;
    private bool isFadingOut = false;

    public enum FadeOutType
    {
        Linear,
        Logarithmic,
        Exponential,
        Polynomial
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isFadingOut)
        {
            fadeOutTimer += Time.deltaTime;
            float t = fadeOutTimer / fadeOutDuration;
            float alpha = 1f;

            switch (fadeOutType)
            {
                case FadeOutType.Linear:
                    alpha = 1 - t;
                    break;
                case FadeOutType.Logarithmic:
                    alpha = 1 - Mathf.Log10(1 + t * 9) / Mathf.Log10(10);
                    break;
                case FadeOutType.Exponential:
                    alpha = Mathf.Exp(-t * 5);
                    break;
                case FadeOutType.Polynomial:
                    alpha = Mathf.Pow(1 - t, polynomialPower);
                    break;
            }

            Color color = spriteRenderer.color;
            color.a = Mathf.Clamp01(alpha);
            spriteRenderer.color = color;

            if (fadeOutTimer >= fadeOutDuration)
            {
                isFadingOut = false;
                Destroy(gameObject);
            }
        }
    }

    public void StartFadeOut()
    {
        isFadingOut = true;
        fadeOutTimer = 0f;
    }
}
