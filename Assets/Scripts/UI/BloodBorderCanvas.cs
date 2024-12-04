using UnityEngine;
using UnityEngine.UI;

public class BloodBorderCanvas : MonoBehaviour
{
    // Singleton instance for global access
    public static BloodBorderCanvas Instance { get; private set; }

    public float startScale = 1.3f;
    public float finishScale = 1.3f;

    [Tooltip("The Image component representing the blood border.")]
    public Image BloodBorderImage;

    [Tooltip("The duration for the blood effect in seconds.")]
    public float BloodDuration = 1.0f;

    private Vector3 initialScale;
    private Vector3 targetScale;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SetImageAlpha(0f);

        initialScale = new Vector3(startScale, startScale, 1);  // Default scale
        targetScale = new Vector3(finishScale, finishScale, 1);   // Target scale when effect is triggered
    }

    private void Update()
    {

    }

    // Call this to trigger the blood effect
    public void DoBlood()
    {
        if (BloodBorderImage == null)
        {
            Debug.LogWarning("BloodBorderImage is not assigned.");
            return;
        }

        // Reset scale and alpha
        BloodBorderImage.rectTransform.localScale = targetScale;
        SetImageAlpha(1.0f);  // Fully visible

        // Stop any ongoing scaling/fading coroutines
        StopAllCoroutines();

        // Start the scaling and fading effect
        StartCoroutine(ScaleAndFadeOut());
    }

    private System.Collections.IEnumerator ScaleAndFadeOut()
    {
        float elapsedTime = 0f;

        while (elapsedTime < BloodDuration)
        {
            elapsedTime += Time.deltaTime;

            // Lerp the scale back to initial scale over BloodDuration
            BloodBorderImage.rectTransform.localScale = Vector3.Lerp(targetScale, initialScale, elapsedTime / BloodDuration);

            // Lerp the alpha to fade out over BloodDuration
            float alpha = Mathf.Lerp(1.0f, 0.0f, elapsedTime / BloodDuration);
            SetImageAlpha(alpha);

            yield return null;
        }

        // Ensure the scale is back to 2, 2, 1 and the alpha is fully transparent
        BloodBorderImage.rectTransform.localScale = initialScale;
        SetImageAlpha(0.0f);
    }

    // Helper method to set the alpha of the BloodBorderImage
    private void SetImageAlpha(float alpha)
    {
        Color color = BloodBorderImage.color;
        color.a = alpha;
        BloodBorderImage.color = color;
    }
}
