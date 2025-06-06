using System.Collections.Generic;
using UnityEngine;

public enum FadeType
{
    Linear,
    Logarithmic,
    Exponential,
    Polynomial
}

public class PopupTextManager : MonoBehaviour
{
    public static PopupTextManager Instance;

    [Header("Settings")]
    public GameObject textPrefab;
    public FadeType fadeType = FadeType.Linear;
    public float fadeDuration = 1f;
    public Vector3 moveDirection = Vector3.up;
    public float moveSpeed = 1f; // Parameter to control the move speed
    public int poolSize = 20;

    private Queue<PopupTextCanvas> textPool;
    private GameObject poolParent;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializePool();
    }

    private void InitializePool()
    {
        textPool = new Queue<PopupTextCanvas>();

        // Create a persistent parent GameObject
        poolParent = new GameObject("PopupTextPool");
        DontDestroyOnLoad(poolParent);

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(textPrefab, poolParent.transform);
            PopupTextCanvas popupTextCanvas = obj.GetComponent<PopupTextCanvas>();
            obj.SetActive(false);
            textPool.Enqueue(popupTextCanvas);
        }
    }

    public void PopupText(string text, Vector3 position, float fontSize, Color fontColor, float randomizePosition = 0f, float polynomialFactor = 3f)
    {
        if (textPool.Count > 0)
        {
            PopupTextCanvas popupTextCanvas = textPool.Dequeue();
            popupTextCanvas.gameObject.SetActive(true);

            // Randomize position
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector3 randomOffset = new Vector3(randomDirection.x, randomDirection.y, 0) * Random.Range(0, randomizePosition);
            Vector3 randomizedPosition = position + randomOffset;

            popupTextCanvas.Initialize(text, randomizedPosition, fontSize, fontColor, fadeType, fadeDuration, moveDirection, moveSpeed, this, polynomialFactor);
        }
    }

    public void ReturnToPool(PopupTextCanvas popupTextCanvas)
    {
        popupTextCanvas.gameObject.SetActive(false);
        textPool.Enqueue(popupTextCanvas);
    }
}