using UnityEngine;

public class RandomSpriteSelector : MonoBehaviour
{
    [System.Serializable]
    public class WeightedSprite
    {
        public Sprite sprite; // The sprite to use
        public int weight;    // The weight of this sprite (higher means more likely)
    }

    [SerializeField] private WeightedSprite[] sprites; // Drag your sprites and set weights here in the Inspector
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (sprites.Length > 0)
        {
            spriteRenderer.sprite = GetRandomWeightedSprite();
        }
    }

    private Sprite GetRandomWeightedSprite()
    {
        int totalWeight = 0;

        // Calculate the total weight
        foreach (var weightedSprite in sprites)
        {
            totalWeight += weightedSprite.weight;
        }

        // Pick a random value
        int randomValue = Random.Range(0, totalWeight);

        // Find which sprite corresponds to the random value
        foreach (var weightedSprite in sprites)
        {
            if (randomValue < weightedSprite.weight)
            {
                return weightedSprite.sprite;
            }
            randomValue -= weightedSprite.weight;
        }

        // Fallback (this shouldn't happen if weights are set correctly)
        return sprites[0].sprite;
    }
}
