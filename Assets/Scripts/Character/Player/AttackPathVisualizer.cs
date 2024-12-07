using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AttackPathVisualizer : MonoBehaviour
{
    // Shared parameters for both shapes
    [Header("General")]
    [SerializeField] public bool useCircle = true; // Toggle between circle and rectangle
    [SerializeField] public float borderThickness = 0.1f; // Thickness of the border
    [SerializeField] public Color fillColor = Color.white; // Fill color
    [SerializeField] public Color borderColor = new Color(1f, 0.5f, 0f); // Border color
    [SerializeField] public Vector2 forwardDirection = Vector2.up; // Forward direction
    [SerializeField] public PlayerPrediction playerPrediction;

    // Circle-specific parameters
    [Header("Circle Target Path Params")]
    [SerializeField] public float innerRadius = 0f; // Inner radius for donuts
    [SerializeField] public float outerRadius = 2f; // Outer radius of the circle
    [SerializeField] public int segments = 64; // Smoothness of the circle
    [SerializeField] public float angle = 360f; // Angle of the sector (0-360)

    // Rectangle-specific parameters
    [Header("Rectangle Target Path Params")]
    [SerializeField] public float innerStartPoint = 1f; // Distance from transform.position where the rectangle starts
    [SerializeField] public float outerFinishPoint = 5f; // Distance where the rectangle ends
    [SerializeField] public float width = 2f; // Width of the rectangle

    // functions to use in code
    [HideInInspector] public bool useParentPositionAsStart = true;
    [HideInInspector] public Vector3 customStartPosition;

    // Cached references for optimization
    private MeshFilter fillMeshFilter;
    [HideInInspector] public MeshRenderer fillMeshRenderer;
    private MeshFilter borderMeshFilter;
    [HideInInspector] public MeshRenderer borderMeshRenderer;

    [SerializeField] private Material attackPathMaterial_Fill;
    [SerializeField] private Material attackPathMaterial_Border;

    private void Awake()
    {
        InitializeMeshObjects();
    }

    private void Start()
    {
        SetMeshVisible(false);
    }

    private void OnDestroy()
    {
        if (fillMeshFilter != null && fillMeshFilter.sharedMesh != null)
        {
            Destroy(fillMeshFilter.sharedMesh);
        }

        if (borderMeshFilter != null && borderMeshFilter.sharedMesh != null)
        {
            Destroy(borderMeshFilter.sharedMesh);
        }

        if (fillMeshFilter != null) Destroy(fillMeshFilter.gameObject);

        if (borderMeshFilter != null) Destroy(borderMeshFilter.gameObject);
    }

    private void Update()
    {
        if (Bootstrap.IsServer()) return;

        if (useCircle)
        {
            UpdateCircleFill();
            UpdateCircleBorder();
        }
        else
        {
            UpdateRectangleFill();
            UpdateRectangleBorder();
        }
    }

    public void SetMeshVisible(bool isVisible)
    {
        fillMeshFilter.gameObject.SetActive(isVisible);
        borderMeshFilter.gameObject.SetActive(isVisible);
    }

    private void InitializeMeshObjects()
    {
        // Fill object
        var fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(transform);
        fillObject.transform.localPosition = Vector3.zero;

        fillMeshFilter = fillObject.AddComponent<MeshFilter>();
        fillMeshRenderer = fillObject.AddComponent<MeshRenderer>();
        //fillMeshRenderer.material = CreateSharedMaterial(fillColor);
        fillMeshRenderer.material = attackPathMaterial_Fill;
        fillMeshRenderer.sortingLayerName = "Shadows";
        fillMeshRenderer.sortingOrder = 2;

        // Border object
        var borderObject = new GameObject("Border");
        borderObject.transform.SetParent(transform);
        borderObject.transform.localPosition = Vector3.zero;

        borderMeshFilter = borderObject.AddComponent<MeshFilter>();
        borderMeshRenderer = borderObject.AddComponent<MeshRenderer>();
        //borderMeshRenderer.material = CreateSharedMaterial(borderColor);
        borderMeshRenderer.material = attackPathMaterial_Border;
        borderMeshRenderer.sortingLayerName = "Shadows";
        borderMeshRenderer.sortingOrder = 3;
    }

    private static Dictionary<Color, Material> materialCache = new Dictionary<Color, Material>();

    private static Material CreateSharedMaterial(Color color)
    {
        if (!materialCache.TryGetValue(color, out Material material))
        {
            material = new Material(Shader.Find("Sprites/Default"));
            material.color = color;
            materialCache[color] = material;
        }
        return material;
    }

    // ** Circle Updating Methods **
    private void UpdateCircleFill()
    {
        if (fillMeshFilter.sharedMesh == null) fillMeshFilter.sharedMesh = new Mesh();
        fillMeshFilter.sharedMesh.Clear();
        GenerateSectorMesh(fillMeshFilter.sharedMesh,
            innerRadius, outerRadius, angle,
            segments, forwardDirection,
            useParentPositionAsStart ? transform.parent.position : customStartPosition);
    }

    private void UpdateCircleBorder()
    {
        if (borderMeshFilter.sharedMesh == null) borderMeshFilter.sharedMesh = new Mesh();
        borderMeshFilter.sharedMesh.Clear();
        GenerateSectorBorderMesh(borderMeshFilter.sharedMesh, innerRadius, outerRadius, angle,
            borderThickness, segments, forwardDirection,
            useParentPositionAsStart ? transform.parent.position : customStartPosition);
    }

    private void UpdateRectangleFill()
    {
        if (fillMeshFilter.sharedMesh == null) fillMeshFilter.sharedMesh = new Mesh();
        fillMeshFilter.sharedMesh.Clear();
        GenerateRectangleMesh(fillMeshFilter.sharedMesh, innerStartPoint, outerFinishPoint, width,
            forwardDirection,
            useParentPositionAsStart ? transform.parent.position : customStartPosition);
    }

    private void UpdateRectangleBorder()
    {
        if (borderMeshFilter.sharedMesh == null) borderMeshFilter.sharedMesh = new Mesh();
        borderMeshFilter.sharedMesh.Clear();
        GenerateRectangleBorderMesh(borderMeshFilter.sharedMesh, innerStartPoint, outerFinishPoint, width,
            forwardDirection,
            useParentPositionAsStart ? transform.parent.position : customStartPosition,
            borderThickness);
    }

    private void GenerateRectangleMesh(Mesh mesh, float innerStart, float outerFinish, float width,
        Vector2 worldDirection, Vector3 worldPosition)
    {
        // Convert world position to local space relative to the parent
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

        // Convert world direction to local space relative to the parent
        Vector3 localDirection3D = transform.InverseTransformDirection(new Vector3(worldDirection.x, worldDirection.y, 0));
        Vector2 localDirection = new Vector2(localDirection3D.x, localDirection3D.y).normalized;

        // Create perpendicular vector for width in local space
        Vector2 perpendicular = new Vector2(-localDirection.y, localDirection.x) * (width / 2f);

        // Calculate rectangle vertices in local space
        Vector3[] localVertices = new Vector3[4];
        localVertices[0] = localPosition + (Vector3)(localDirection * innerStart - perpendicular); // Bottom-left
        localVertices[1] = localPosition + (Vector3)(localDirection * innerStart + perpendicular); // Bottom-right
        localVertices[2] = localPosition + (Vector3)(localDirection * outerFinish + perpendicular); // Top-right
        localVertices[3] = localPosition + (Vector3)(localDirection * outerFinish - perpendicular); // Top-left

        // Define triangles for the mesh
        int[] triangles = { 0, 1, 2, 2, 3, 0 };

        // Assign vertices and triangles to the mesh
        mesh.vertices = localVertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }


    private void GenerateRectangleBorderMesh(Mesh mesh, float innerStart, float outerFinish, float width,
        Vector2 worldDirection, Vector3 worldPosition, float borderThickness)
    {
        // Convert world position to local space relative to the parent
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

        // Convert world direction to local space relative to the parent
        Vector3 localDirection3D = transform.InverseTransformDirection(new Vector3(worldDirection.x, worldDirection.y, 0));
        Vector2 localDirection = new Vector2(localDirection3D.x, localDirection3D.y).normalized;

        // Extend dimensions to account for the border
        float extendedInnerStart = innerStart - borderThickness;
        float extendedOuterFinish = outerFinish + borderThickness;
        float extendedWidth = width + borderThickness * 2;

        // Create perpendicular vector for width in local space
        Vector2 perpendicular = new Vector2(-localDirection.y, localDirection.x);

        // Calculate border vertices in local space
        Vector3[] localVertices = new Vector3[8];
        localVertices[0] = localPosition + (Vector3)(localDirection * extendedInnerStart - perpendicular * (extendedWidth / 2f)); // Bottom-left outer
        localVertices[1] = localPosition + (Vector3)(localDirection * extendedInnerStart + perpendicular * (extendedWidth / 2f)); // Bottom-right outer
        localVertices[2] = localPosition + (Vector3)(localDirection * innerStart + perpendicular * (width / 2f)); // Bottom-right inner
        localVertices[3] = localPosition + (Vector3)(localDirection * innerStart - perpendicular * (width / 2f)); // Bottom-left inner

        localVertices[4] = localPosition + (Vector3)(localDirection * outerFinish - perpendicular * (width / 2f)); // Top-left inner
        localVertices[5] = localPosition + (Vector3)(localDirection * outerFinish + perpendicular * (width / 2f)); // Top-right inner
        localVertices[6] = localPosition + (Vector3)(localDirection * extendedOuterFinish + perpendicular * (extendedWidth / 2f)); // Top-right outer
        localVertices[7] = localPosition + (Vector3)(localDirection * extendedOuterFinish - perpendicular * (extendedWidth / 2f)); // Top-left outer

        // Define triangles for the border outline
        int[] triangles = {
                0, 1, 2, 2, 3, 0, // Bottom border
                1, 6, 5, 5, 2, 1, // Right border
                6, 7, 4, 4, 5, 6, // Top border
                7, 0, 3, 3, 4, 7  // Left border
            };

        // Assign vertices and triangles to the mesh
        mesh.vertices = localVertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }

    private void GenerateSectorMesh(Mesh mesh, float innerRadius, float outerRadius, float angle, int segments, Vector2 worldDirection, Vector3 worldPosition)
    {
        // Convert world position to local space relative to the parent
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

        // Convert world direction to local space relative to the parent
        Vector3 localDirection3D = transform.InverseTransformDirection(new Vector3(worldDirection.x, worldDirection.y, 0));
        Vector2 localDirection = new Vector2(localDirection3D.x, localDirection3D.y).normalized;

        int verticesCount = (segments + 1) * 2; // Inner and outer vertices for each segment
        Vector3[] vertices = new Vector3[verticesCount];
        int[] triangles = new int[segments * 6];

        float angleStep = angle / segments * Mathf.Deg2Rad;

        // Align the sector's angle with the local direction
        float forwardAngle = Mathf.Atan2(localDirection.y, localDirection.x);

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = forwardAngle - angle * Mathf.Deg2Rad / 2 + i * angleStep;

            // Calculate direction using the adjusted angle
            Vector3 direction = new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0);

            // Inner and outer vertices
            vertices[i] = localPosition + direction * innerRadius;
            vertices[segments + 1 + i] = localPosition + direction * outerRadius;

            if (i < segments)
            {
                // Create triangles
                int start = i;
                int innerNext = i + 1;
                int outer = segments + 1 + i;
                int outerNext = segments + 2 + i;

                // First triangle
                triangles[i * 6] = start;
                triangles[i * 6 + 1] = innerNext;
                triangles[i * 6 + 2] = outer;

                // Second triangle
                triangles[i * 6 + 3] = outer;
                triangles[i * 6 + 4] = innerNext;
                triangles[i * 6 + 5] = outerNext;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }



    private void GenerateSectorBorderMesh(Mesh mesh, float innerRadius, float outerRadius, float angle, float borderThickness, int segments, Vector2 worldDirection, Vector3 worldPosition)
    {
        // Convert world position to local space relative to the parent
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

        // Convert world direction to local space relative to the parent
        Vector3 localDirection3D = transform.InverseTransformDirection(new Vector3(worldDirection.x, worldDirection.y, 0));
        Vector2 localDirection = new Vector2(localDirection3D.x, localDirection3D.y).normalized;

        float extendedInnerRadius = innerRadius - borderThickness;
        float extendedOuterRadius = outerRadius + borderThickness;

        Vector3[] vertices = new Vector3[(segments + 1) * 4];
        int[] triangles = new int[segments * 12];

        float angleStep = angle / segments * Mathf.Deg2Rad;

        // Align the sector's angle with the local direction
        float forwardAngle = Mathf.Atan2(localDirection.y, localDirection.x);

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = forwardAngle - angle * Mathf.Deg2Rad / 2 + i * angleStep;

            Vector3 direction = new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0);

            // Outer border vertices
            vertices[i] = localPosition + direction * extendedOuterRadius;

            // Outer circle vertices
            vertices[segments + 1 + i] = localPosition + direction * outerRadius;

            // Inner circle vertices
            vertices[(segments + 1) * 2 + i] = localPosition + direction * innerRadius;

            // Inner border vertices
            vertices[(segments + 1) * 3 + i] = localPosition + direction * extendedInnerRadius;

            if (i < segments)
            {
                int baseIndex = i * 12;

                // Outer border
                triangles[baseIndex] = i;
                triangles[baseIndex + 1] = i + 1;
                triangles[baseIndex + 2] = segments + 1 + i;

                triangles[baseIndex + 3] = segments + 1 + i;
                triangles[baseIndex + 4] = i + 1;
                triangles[baseIndex + 5] = segments + 2 + i;

                // Inner border
                triangles[baseIndex + 6] = (segments + 1) * 3 + i;
                triangles[baseIndex + 7] = (segments + 1) * 2 + i + 1;
                triangles[baseIndex + 8] = (segments + 1) * 2 + i;

                triangles[baseIndex + 9] = (segments + 1) * 3 + i;
                triangles[baseIndex + 10] = (segments + 1) * 3 + i + 1;
                triangles[baseIndex + 11] = (segments + 1) * 2 + i + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }
}