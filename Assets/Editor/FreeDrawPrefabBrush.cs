using UnityEngine;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Free Draw Prefab Brush", menuName = "Brushes/Free Draw Prefab Brush")]
[CustomGridBrush(false, true, false, "Free Draw Prefab Brush")]
public class FreeDrawPrefabBrush : GridBrush
{
    public GameObject prefab;
    public Vector3 offset;

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
        Vector3 worldPosition = grid.CellToWorld(position) + offset;

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
