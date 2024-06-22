using UnityEditor;
using UnityEngine;

public class SpriteExploder : MonoBehaviour
{
    public Texture2D spriteSheet; // The sprite sheet texture
    public GameObject fragmentPrefab; // The fragment prefab
    public int spriteWidth = 36; // Width of the original sprite
    public int spriteHeight = 36; // Height of the original sprite
    public int cellSize = 6; // Size of each grid cell (fragment)
    public float pixelsPerUnit = 36f; // Conversion from pixels to Unity units
    public float explosionForce = 200f; // Force of the explosion
    public Transform explosionPoint;
    public float gravityScale = 1f; // Gravity scale for the fragments
    public float fadeOutDuration = 2f; // Duration for the fade-out effect
    public FragmentController.FadeOutType fadeOutType = FragmentController.FadeOutType.Linear; // Fade out type
    public int polynomialPower = 2; // Power for polynomial fade-out
    public float initialForceStrength = 1f; // Initial force strength of the explosion
    public float randomizationFactor = 0.2f; // Factor for randomizing the initial force

    private Sprite[] sprites; // Array to store the individual sprites
    private GameObject[] fragments; // Array to store fragment GameObjects

    void Start()
    {
        LoadSprites();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button to explode
        {
            Explode(explosionPoint.transform.position);
        }
        if (Input.GetMouseButtonDown(1)) // Right mouse button to reset
        {
            GenerateFragments();
        }
    }

    void OnDisable()
    {
        ClearFragments();
    }

    // Load sprites from the sprite sheet
    private void LoadSprites()
    {
        string path = AssetDatabase.GetAssetPath(spriteSheet);
        Object[] loadedObjects = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
        sprites = new Sprite[loadedObjects.Length];

        for (int i = 0; i < loadedObjects.Length; i++)
        {
            sprites[i] = loadedObjects[i] as Sprite;
        }

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("No sprites found in the specified sprite sheet.");
        }
        else
        {
            Debug.Log("Loaded " + sprites.Length + " sprites from the sprite sheet.");
        }
    }

    // Generates the fragments and assigns them to child game objects
    public void GenerateFragments()
    {
        ClearFragments();
        LoadSprites();

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("No sprites loaded. Make sure the sprite sheet is sliced correctly.");
            return;
        }

        if (fragmentPrefab == null)
        {
            Debug.LogError("Fragment prefab is not assigned.");
            return;
        }

        int columns = spriteWidth / cellSize;
        int rows = spriteHeight / cellSize;
        float unitCellSize = cellSize / pixelsPerUnit;

        Debug.Log("Generating fragments: " + columns + " columns, " + rows + " rows.");

        fragments = new GameObject[columns * rows];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                int index = (rows - 1 - y) * columns + x; // Adjusted index to start from the bottom row
                if (index < sprites.Length)
                {
                    GameObject fragment = Instantiate(fragmentPrefab, transform);
                    fragment.name = "Fragment_" + x + "_" + y;
                    fragment.transform.localPosition = new Vector3(x * unitCellSize, y * unitCellSize, 0);
                    fragments[index] = fragment;

                    SpriteRenderer sr = fragment.GetComponent<SpriteRenderer>();
                    sr.sprite = sprites[index];

                    Rigidbody2D rb = fragment.GetComponent<Rigidbody2D>();
                    rb.gravityScale = 0; // Initially disable gravity

                    FragmentController fc = fragment.GetComponent<FragmentController>();
                    fc.fadeOutDuration = fadeOutDuration;
                    fc.fadeOutType = fadeOutType;
                    fc.polynomialPower = polynomialPower;
                }
                else
                {
                    Debug.LogError("Index out of range: " + index);
                }
            }
        }
    }

    // Clear existing fragments
    private void ClearFragments()
    {
        if (fragments != null)
        {
            for (int i = 0; i < fragments.Length; i++)
            {
                if (fragments[i] != null)
                {
                    DestroyImmediate(fragments[i]);
                }
            }
            fragments = null;
        }
    }

    // Explode the fragments from a specified point
    public void Explode(Vector2 explosionPoint)
    {
        foreach (Transform fragment in transform)
        {
            if (fragment.GetComponent<FragmentController>() == null) continue;

            Rigidbody2D rb = fragment.GetComponent<Rigidbody2D>();
            Vector2 direction = (fragment.localPosition - (Vector3)explosionPoint).normalized;
            Vector2 randomizedDirection = direction + new Vector2(Random.Range(-randomizationFactor, randomizationFactor), Random.Range(-randomizationFactor, randomizationFactor));
            rb.AddForce(randomizedDirection * explosionForce * initialForceStrength);
            rb.gravityScale = gravityScale; // Enable gravity after applying explosion force

            FragmentController fc = fragment.GetComponent<FragmentController>();
            if (fc != null)
            {
                fc.StartFadeOut();
            }
        }
    }
}
