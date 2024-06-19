using UnityEngine;
using TMPro;

public class PopupTextCanvas : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private RectTransform rectTransform;
    private PopupTextManager manager;

    private Vector3 moveDirection;
    private float moveSpeed; // Variable to control the move speed
    private float fadeDuration;
    private FadeType fadeType;
    private float timer;
    private Color originalColor;
    private float polynomialFactor = 3f; // Default polynomial factor

    private void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(string text, Vector3 position, float fontSize, Color fontColor, FadeType fadeType, float fadeDuration, Vector3 moveDirection, float moveSpeed, PopupTextManager manager, float polynomialFactor = 3f)
    {
        rectTransform.position = position;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = fontColor;
        this.fadeType = fadeType;
        this.fadeDuration = fadeDuration;
        this.moveDirection = moveDirection;
        this.moveSpeed = moveSpeed; // Assign the move speed
        this.manager = manager;
        this.polynomialFactor = polynomialFactor; // Assign the polynomial factor
        timer = 0f;
        originalColor = fontColor;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer > fadeDuration)
        {
            manager.ReturnToPool(this);
            return;
        }

        float fadeAmount = CalculateFade(timer / fadeDuration);
        textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, fadeAmount);
        rectTransform.position += moveDirection * moveSpeed * Time.deltaTime; // Apply move speed
    }

    private float CalculateFade(float t)
    {
        switch (fadeType)
        {
            case FadeType.Linear:
                return 1f - t;
            case FadeType.Logarithmic:
                return Mathf.Log(1f - t + 1f);
            case FadeType.Exponential:
                return 1f - Mathf.Pow(t, 2);
            case FadeType.Polynomial:
                return 1f - Mathf.Pow(t, polynomialFactor); // Use polynomial factor
            default:
                return 1f - t;
        }
    }
}
