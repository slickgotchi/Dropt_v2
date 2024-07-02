using UnityEngine;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Free Draw Prefab Brush", menuName = "Brushes/Free Draw Prefab Brush")]
[CustomGridBrush(false, true, false, "Free Draw Prefab Brush")]
public class FreeDrawPrefabBrush : GridBrush
{
    private const float kMinFactor = .0001f;

    public GameObject prefab;
    public Vector3 offset;
    public float cellSize = 1;

    public override void Paint(GridLayout grid, GameObject brushTarget, Vector3Int position)
    {
        if (prefab == null)
        {
            Debug.LogWarning("No prefab assigned to the brush.");
            return;
        }

        // Ensure the brushTarget is a GameObject and has a Grid component
        if (brushTarget.layer == 31) // Prevent painting on the preview tilemap
            return;

        // Calculate the world position for the prefab
        Vector3 worldPosition = GetWorldPosition(grid, position);

        Transform objectInCell = GetObjectInCell(brushTarget.transform, worldPosition);

        if (null != objectInCell)
            return;

        // Instantiate the prefab at the calculated position
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.transform.position = worldPosition;
        instance.transform.SetParent(brushTarget.transform);
        Undo.RegisterCreatedObjectUndo(instance, "Paint GameObject");
    }
    
    public override void BoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
    {
        if (cellSize <= 0)
        {
            //Default implementation
            base.BoxFill(gridLayout, brushTarget, position);
            return;
        }

        if (brushTarget == null)
            return;

        var map = brushTarget.GetComponent<Tilemap>();
        if (map == null)
            return;

        foreach (var location in position.allPositionsWithin)
        {
            var worldPosition = GetWorldPosition(gridLayout, location);
            Transform objectInCell = GetObjectInCell(brushTarget.transform, worldPosition);

            if (null != objectInCell)
                continue;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = worldPosition;
            instance.transform.SetParent(brushTarget.transform);
            Undo.RegisterCreatedObjectUndo(instance, "Paint GameObject");
        }
    }

    public override void Erase(GridLayout grid, GameObject brushTarget, Vector3Int position)
    {
        Transform objectInCell = GetObjectInCell(brushTarget.transform, GetWorldPosition(grid, position));
        if (objectInCell != null)
        {
            Undo.DestroyObjectImmediate(objectInCell.gameObject);
            Debug.Log("Object erased at position: " + position);
        }
    }

    private Transform GetObjectInCell(Transform parent, Vector3 position)
    {
        foreach (Transform child in parent)
        {
            if (Vector3.Distance(position, child.position) <= kMinFactor)
            {
                return child;
            }
        }
        return null;
    }

    private static float RandomSystem(System.Random random, float min, float max)
    {
        return (float)random.NextDouble() * (max + kMinFactor - min) + min;
    }

    private Vector3 GetWorldPosition(GridLayout grid, Vector3Int position)
    {
        Vector3 worldPosition = grid.CellToWorld(position);

        if (cellSize <= 0)
        {
            //Default implementation
            return worldPosition + offset;
        }

        //Extended implementation with tile offset randomization

        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.y / cellSize);

        System.Random r = new System.Random(x + y);
        var currentOffset = offset;

        currentOffset.x = RandomSystem(r, 0, offset.x);
        currentOffset.y = RandomSystem(r, 0, offset.y);

        worldPosition.x = x * cellSize;
        worldPosition.y = y * cellSize;

        return worldPosition + currentOffset;
    }
}