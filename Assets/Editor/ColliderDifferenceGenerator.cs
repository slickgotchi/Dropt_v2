using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Clipper2Lib;

public class ColliderDifferenceGenerator : EditorWindow
{
    public Collider2D baseCollider;
    public Collider2D subtractionCollider;

    // Parameters for circle and capsule resolution
    private const int circleResolution = 30;
    private const int capsuleResolution = 15;
    private const double scaleFactor = 1.0; // Using double precision for Clipper2

    [MenuItem("Tools/Generate Collider Difference")]
    public static void ShowWindow()
    {
        GetWindow<ColliderDifferenceGenerator>("Collider Difference Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Select Colliders", EditorStyles.boldLabel);

        // Allow selection of any Collider2D
        baseCollider = (Collider2D)EditorGUILayout.ObjectField("Base Collider", baseCollider, typeof(Collider2D), true);
        subtractionCollider = (Collider2D)EditorGUILayout.ObjectField("Subtraction Collider", subtractionCollider, typeof(Collider2D), true);

        if (GUILayout.Button("Generate and Save Difference"))
        {
            if (baseCollider == null || subtractionCollider == null)
            {
                Debug.LogWarning("Both Base Collider and Subtraction Collider need to be assigned.");
            }
            else
            {
                GenerateAndSaveDifference(baseCollider, subtractionCollider);
            }
        }
    }

    void GenerateAndSaveDifference(Collider2D baseCollider, Collider2D subtractionCollider)
    {
        // Get paths (polygons) from the two colliders
        PathsD basePaths = GetPathsFromCollider(baseCollider);
        PathsD subtractionPaths = GetPathsFromCollider(subtractionCollider);

        // Prepare the Clipper object to perform the difference operation
        ClipperD clipper = new ClipperD();
        clipper.AddSubject(basePaths);
        clipper.AddClip(subtractionPaths);

        // Perform the difference operation
        PathsD solution = new PathsD();
        clipper.Execute(ClipType.Difference, FillRule.NonZero, solution);

        // Create and save the resulting collider
        CreateAndSaveResultingCollider(solution);
    }

    PathsD GetPathsFromCollider(Collider2D collider)
    {
        if (collider is PolygonCollider2D polygonCollider)
        {
            return GetPathsFromPolygonCollider(polygonCollider);
        }
        else if (collider is BoxCollider2D boxCollider)
        {
            return GetPathsFromBoxCollider(boxCollider);
        }
        else if (collider is CircleCollider2D circleCollider)
        {
            return GetPathsFromCircleCollider(circleCollider);
        }
        else if (collider is CapsuleCollider2D capsuleCollider)
        {
            return GetPathsFromCapsuleCollider(capsuleCollider);
        }
        else if (collider is EdgeCollider2D edgeCollider)
        {
            return GetPathsFromEdgeCollider(edgeCollider);
        }
        else if (collider is TilemapCollider2D tilemapCollider)
        {
            return GetPathsFromTilemapCollider(tilemapCollider);
        }

        Debug.LogError("Unsupported Collider2D type: " + collider.GetType());
        return new PathsD();
    }

    PathsD GetPathsFromPolygonCollider(PolygonCollider2D polygonCollider)
    {
        PathsD paths = new PathsD();

        for (int i = 0; i < polygonCollider.pathCount; i++)
        {
            Vector2[] pathPoints = polygonCollider.GetPath(i);
            PathD pathD = new PathD();

            foreach (Vector2 point in pathPoints)
            {
                pathD.Add(new PointD(point.x, point.y));
            }
            paths.Add(pathD);
        }

        return paths;
    }

    PathsD GetPathsFromBoxCollider(BoxCollider2D boxCollider)
    {
        PathsD paths = new PathsD();
        PathD pathD = new PathD();

        Vector2 size = boxCollider.size;
        Vector2 offset = boxCollider.offset;

        // Get the four corners of the box
        Vector2 bottomLeft = (Vector2)boxCollider.transform.position + offset + new Vector2(-size.x / 2, -size.y / 2);
        Vector2 bottomRight = (Vector2)boxCollider.transform.position + offset + new Vector2(size.x / 2, -size.y / 2);
        Vector2 topRight = (Vector2)boxCollider.transform.position + offset + new Vector2(size.x / 2, size.y / 2);
        Vector2 topLeft = (Vector2)boxCollider.transform.position + offset + new Vector2(-size.x / 2, size.y / 2);

        pathD.Add(new PointD(bottomLeft.x, bottomLeft.y));
        pathD.Add(new PointD(bottomRight.x, bottomRight.y));
        pathD.Add(new PointD(topRight.x, topRight.y));
        pathD.Add(new PointD(topLeft.x, topLeft.y));

        paths.Add(pathD);
        return paths;
    }

    PathsD GetPathsFromCircleCollider(CircleCollider2D circleCollider)
    {
        PathsD paths = new PathsD();
        PathD pathD = new PathD();

        Vector2 center = (Vector2)circleCollider.transform.position + circleCollider.offset;
        float radius = circleCollider.radius;

        // Generate points around the circle
        for (int i = 0; i < circleResolution; i++)
        {
            float angle = i * Mathf.PI * 2f / circleResolution;
            Vector2 point = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius) + center;
            pathD.Add(new PointD(point.x, point.y));
        }

        paths.Add(pathD);
        return paths;
    }

    PathsD GetPathsFromCapsuleCollider(CapsuleCollider2D capsuleCollider)
    {
        PathsD paths = new PathsD();
        PathD pathD = new PathD();

        Vector2 center = (Vector2)capsuleCollider.transform.position + capsuleCollider.offset;
        float radius = capsuleCollider.size.x / 2;
        float height = capsuleCollider.size.y;

        // Generate points for the top arc
        for (int i = 0; i < capsuleResolution; i++)
        {
            float angle = Mathf.PI * i / (capsuleResolution - 1);
            Vector2 point = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius + height / 2) + center;
            pathD.Add(new PointD(point.x, point.y));
        }

        // Add the side points
        pathD.Add(new PointD(center.x + radius, center.y - height / 2));
        pathD.Add(new PointD(center.x - radius, center.y - height / 2));

        // Generate points for the bottom arc
        for (int i = 0; i < capsuleResolution; i++)
        {
            float angle = Mathf.PI * i / (capsuleResolution - 1) + Mathf.PI;
            Vector2 point = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius - height / 2) + center;
            pathD.Add(new PointD(point.x, point.y));
        }

        paths.Add(pathD);
        return paths;
    }

    PathsD GetPathsFromEdgeCollider(EdgeCollider2D edgeCollider)
    {
        PathsD paths = new PathsD();
        PathD pathD = new PathD();

        Vector2[] points = edgeCollider.points;
        foreach (Vector2 point in points)
        {
            pathD.Add(new PointD(point.x, point.y));
        }

        paths.Add(pathD);
        return paths;
    }

    // New function to handle TilemapCollider2D
    PathsD GetPathsFromTilemapCollider(TilemapCollider2D tilemapCollider)
    {
        PathsD paths = new PathsD();
        Tilemap tilemap = tilemapCollider.GetComponent<Tilemap>();

        // Iterate over all the positions in the tilemap
        foreach (var position in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(position))
            {
                Vector3Int tilePos = position;
                Vector3 worldPos = tilemap.GetCellCenterWorld(tilePos);

                // Assuming the tiles are rectangular, we can generate a rectangle for each tile
                float cellSize = tilemap.cellSize.x; // Assuming square tiles

                PathD pathD = new PathD
                {
                    new PointD(worldPos.x - cellSize / 2, worldPos.y - cellSize / 2),
                    new PointD(worldPos.x + cellSize / 2, worldPos.y - cellSize / 2),
                    new PointD(worldPos.x + cellSize / 2, worldPos.y + cellSize / 2),
                    new PointD(worldPos.x - cellSize / 2, worldPos.y + cellSize / 2)
                };

                paths.Add(pathD);
            }
        }

        return paths;
    }

    void CreateAndSaveResultingCollider(PathsD solution)
    {
        // Create a new GameObject in the scene to hold the resulting collider
        GameObject resultObject = new GameObject("ResultingCollider");
        PolygonCollider2D resultCollider = resultObject.AddComponent<PolygonCollider2D>();

        // Convert solution (Clipper output) back to Unity's Vector2[]
        List<Vector2[]> resultPaths = new List<Vector2[]>();

        foreach (var path in solution)
        {
            Vector2[] resultPath = new Vector2[path.Count];

            for (int i = 0; i < path.Count; i++)
            {
                // Convert from Clipper2's PointD (double) to Unity's Vector2 (float)
                resultPath[i] = new Vector2((float)path[i].x, (float)path[i].y);
            }

            resultPaths.Add(resultPath);
        }

        // Set paths to the PolygonCollider2D
        resultCollider.pathCount = resultPaths.Count;
        for (int i = 0; i < resultPaths.Count; i++)
        {
            resultCollider.SetPath(i, resultPaths[i]);
        }

        Debug.Log("Difference operation complete, resulting collider created.");

        // Prompt the user to save the resulting GameObject as a prefab
        SaveAsPrefab(resultObject);
    }

    void SaveAsPrefab(GameObject obj)
    {
        // Ask for a file path to save the prefab
        string savePath = EditorUtility.SaveFilePanelInProject("Save Resulting Collider Prefab", obj.name, "prefab", "Please enter a file name to save the resulting collider prefab.");

        if (string.IsNullOrEmpty(savePath))
        {
            Debug.LogWarning("Save operation was cancelled.");
            DestroyImmediate(obj); // Destroy the temp GameObject if not saved
            return;
        }

        // Save the resulting GameObject as a prefab
        PrefabUtility.SaveAsPrefabAsset(obj, savePath);
        Debug.Log($"Prefab saved at: {savePath}");

        // Optionally, destroy the GameObject in the scene after saving
        DestroyImmediate(obj);
    }
}
