using UnityEngine;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New GameObject Brush", menuName = "Brushes/GameObject Brush")]
[CustomGridBrush(false, true, false, "GameObject Brush")]
public class GameObjectBrush : GridBrush
{
    public GameObject[] prefabs;
    public Vector3 offset;
    public Vector3 randomOffsetRange;

    private int lastUsedIndex = -1;

    public override void Paint(GridLayout grid, GameObject brushTarget, Vector3Int position)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("No prefabs assigned to the brush.");
            return;
        }

        // Prevent painting on the preview tilemap
        if (brushTarget.layer == 31)
            return;

        // Check if there's already an object at the position
        Transform existingObject = GetObjectInCell(grid, brushTarget.transform, position);
        if (existingObject != null)
        {
            return; // Skip if there is already an object in the cell
        }

        // Select the next prefab (sequentially)
        lastUsedIndex = (lastUsedIndex + 1) % prefabs.Length;
        GameObject prefab = prefabs[lastUsedIndex];

        // Calculate the world position for the prefab with random offset
        Vector3 randomOffset = new Vector3(
            Random.Range(-randomOffsetRange.x, randomOffsetRange.x),
            Random.Range(-randomOffsetRange.y, randomOffsetRange.y),
            Random.Range(-randomOffsetRange.z, randomOffsetRange.z)
        );
        Vector3 worldPosition = grid.CellToWorld(position) + offset + randomOffset;

        // Instantiate the prefab at the calculated position
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.transform.position = worldPosition;
        instance.transform.SetParent(brushTarget.transform);
        Undo.RegisterCreatedObjectUndo(instance, "Paint GameObject");
    }

    public override void Erase(GridLayout grid, GameObject brushTarget, Vector3Int position)
    {
        Transform objectInCell = GetObjectInCell(grid, brushTarget.transform, position);
        if (objectInCell != null)
        {
            Undo.DestroyObjectImmediate(objectInCell.gameObject);
            Debug.Log("Object erased at position: " + position);
        }
    }

    private Transform GetObjectInCell(GridLayout grid, Transform parent, Vector3Int position)
    {
        Vector3 worldPosition = grid.CellToWorld(position);
        foreach (Transform child in parent)
        {
            if (grid.WorldToCell(child.position) == position)
            {
                return child;
            }
        }
        return null;
    }
}
