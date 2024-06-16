using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Random Tile With Chances", menuName = "Tiles/Random Tile With Chances")]
public class RandomTileWithChances : TileBase
{
    [System.Serializable]
    public struct SpriteChance
    {
        public Sprite sprite;
        [Range(0, 1)]
        public float chance;
        public Tile.ColliderType colliderType;
    }

    public SpriteChance[] spriteChances;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        if (spriteChances != null && spriteChances.Length > 0)
        {
            var randomSpriteChance = GetRandomSpriteChance(position);
            tileData.sprite = randomSpriteChance.sprite;
            tileData.color = Color.white;
            tileData.transform = Matrix4x4.identity;
            tileData.gameObject = null;
            tileData.flags = TileFlags.LockTransform;
            tileData.colliderType = randomSpriteChance.colliderType;
        }
    }

    private SpriteChance GetRandomSpriteChance(Vector3Int position)
    {
        float totalChance = 0f;
        foreach (var spriteChance in spriteChances)
        {
            totalChance += spriteChance.chance;
        }

        int hash = position.x * 73856093 ^ position.y * 19349663 ^ position.z * 83492791;
        Random.InitState(hash);
        float randomValue = Random.Range(0f, totalChance);
        float cumulativeChance = 0f;

        foreach (var spriteChance in spriteChances)
        {
            cumulativeChance += spriteChance.chance;
            if (randomValue < cumulativeChance)
            {
                return spriteChance;
            }
        }

        return spriteChances[0];
    }
}
