using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AttackPathVisualizer : MonoBehaviour
{
    // Shared parameters for both shapes
    [Header("General")]
    [SerializeField] public bool useCircle = true; // Toggle between circle and rectangle
    [SerializeField] private float borderThickness = 0.1f; // Thickness of the border
    [SerializeField] private Color fillColor = Color.white; // Fill color
    [SerializeField] private Color borderColor = new Color(1f, 0.5f, 0f); // Border color
    [SerializeField] public Vector2 forwardDirection = Vector2.up; // Forward direction
    [SerializeField] private PlayerPrediction playerPrediction;

    // Circle-specific parameters
    [Header("Circle Target Path Params")]
    [SerializeField] public float innerRadius = 0f; // Inner radius for donuts
    [SerializeField] public float outerRadius = 2f; // Outer radius of the circle
    [SerializeField] public int segments = 64; // Smoothness of the circle
    [SerializeField] public float angle = 360f; // Angle of the sector (0-360)

    // Rectangle-specific parameters
    [Header("Rectangle Target Path Params")]
    [SerializeField] private float innerStartPoint = 1f; // Distance from transform.position where the rectangle starts
    [SerializeField] private float outerFinishPoint = 5f; // Distance where the rectangle ends
    [SerializeField] private float width = 2f; // Width of the rectangle

    // Cached references for optimization
    private MeshFilter fillMeshFilter;
    private MeshRenderer fillMeshRenderer;
    private MeshFilter borderMeshFilter;
    private MeshRenderer borderMeshRenderer;

    private void Awake()
    {
        InitializeMeshObjects();
    }

    private void Start()
    {
        SetMeshVisible(false);
    }

    private void Update()
    {
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
        fillMeshRenderer.material = CreateSharedMaterial(fillColor);
        fillMeshRenderer.sortingLayerName = "Shadows";
        fillMeshRenderer.sortingOrder = 2;

        // Border object
        var borderObject = new GameObject("Border");
        borderObject.transform.SetParent(transform);
        borderObject.transform.localPosition = Vector3.zero;

        borderMeshFilter = borderObject.AddComponent<MeshFilter>();
        borderMeshRenderer = borderObject.AddComponent<MeshRenderer>();
        borderMeshRenderer.material = CreateSharedMaterial(borderColor);
        borderMeshRenderer.sortingLayerName = "Shadows";
        borderMeshRenderer.sortingOrder = 3;
    }

    private static Material CreateSharedMaterial(Color color)
    {
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = color;
        return material;
    }

    // ** Circle Updating Methods **
    private void UpdateCircleFill()
    {
        if (!fillMeshFilter.sharedMesh)
            fillMeshFilter.sharedMesh = new Mesh();
        fillMeshFilter.sharedMesh.Clear();
        fillMeshFilter.sharedMesh = GenerateSectorMesh(innerRadius, outerRadius, angle, segments);
    }

    private void UpdateCircleBorder()
    {
        if (!borderMeshFilter.sharedMesh)
            borderMeshFilter.sharedMesh = new Mesh();
        borderMeshFilter.sharedMesh.Clear();
        borderMeshFilter.sharedMesh = GenerateSectorBorderMesh(innerRadius, outerRadius, angle, borderThickness, segments);
    }

    // ** Rectangle Updating Methods **
    private void UpdateRectangleFill()
    {
        if (!fillMeshFilter.sharedMesh)
            fillMeshFilter.sharedMesh = new Mesh();
        fillMeshFilter.sharedMesh.Clear();
        fillMeshFilter.sharedMesh = GenerateRectangleMesh(innerStartPoint, outerFinishPoint, width, forwardDirection);
    }

    private void UpdateRectangleBorder()
    {
        if (!borderMeshFilter.sharedMesh)
            borderMeshFilter.sharedMesh = new Mesh();
        borderMeshFilter.sharedMesh.Clear();
        borderMeshFilter.sharedMesh = GenerateRectangleBorderMesh(innerStartPoint, outerFinishPoint, width, forwardDirection, borderThickness);
    }

    // ** Rectangle Mesh Generation **
    private Mesh GenerateRectangleMesh(float innerStart, float outerFinish, float width, Vector2 direction)
    {
        Mesh mesh = new Mesh();

        Vector2 normalizedDirection = direction.normalized;
        Vector2 perpendicular = new Vector2(-normalizedDirection.y, normalizedDirection.x) * (width / 2f);

        // Calculate rectangle vertices
        Vector3[] vertices = new Vector3[4];
        vertices[0] = (Vector3)(normalizedDirection * innerStart - perpendicular); // Bottom-left
        vertices[1] = (Vector3)(normalizedDirection * innerStart + perpendicular); // Bottom-right
        vertices[2] = (Vector3)(normalizedDirection * outerFinish + perpendicular); // Top-right
        vertices[3] = (Vector3)(normalizedDirection * outerFinish - perpendicular); // Top-left

        // Define triangles
        int[] triangles = { 0, 1, 2, 2, 3, 0 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    private Mesh GenerateRectangleBorderMesh(float innerStart, float outerFinish, float width, Vector2 direction, float borderThickness)
    {
        Mesh mesh = new Mesh();

        float extendedInnerStart = innerStart - borderThickness;
        float extendedOuterFinish = outerFinish + borderThickness;
        float extendedWidth = width + borderThickness * 2;

        Vector2 normalizedDirection = direction.normalized;
        Vector2 perpendicular = new Vector2(-normalizedDirection.y, normalizedDirection.x);

        // Calculate border vertices
        Vector3[] vertices = new Vector3[8];
        vertices[0] = (Vector3)(normalizedDirection * extendedInnerStart - perpendicular * (extendedWidth / 2f)); // Bottom-left outer
        vertices[1] = (Vector3)(normalizedDirection * extendedInnerStart + perpendicular * (extendedWidth / 2f)); // Bottom-right outer
        vertices[2] = (Vector3)(normalizedDirection * innerStart + perpendicular * (width / 2f)); // Bottom-right inner
        vertices[3] = (Vector3)(normalizedDirection * innerStart - perpendicular * (width / 2f)); // Bottom-left inner

        vertices[4] = (Vector3)(normalizedDirection * outerFinish - perpendicular * (width / 2f)); // Top-left inner
        vertices[5] = (Vector3)(normalizedDirection * outerFinish + perpendicular * (width / 2f)); // Top-right inner
        vertices[6] = (Vector3)(normalizedDirection * extendedOuterFinish + perpendicular * (extendedWidth / 2f)); // Top-right outer
        vertices[7] = (Vector3)(normalizedDirection * extendedOuterFinish - perpendicular * (extendedWidth / 2f)); // Top-left outer

        // Define triangles for the border outline
        int[] triangles = {
        0, 1, 2, 2, 3, 0, // Bottom border
        1, 6, 5, 5, 2, 1, // Right border
        6, 7, 4, 4, 5, 6, // Top border
        7, 0, 3, 3, 4, 7  // Left border
    };

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        return mesh;
    }



    private Mesh GenerateSectorMesh(float innerRadius, float outerRadius, float angle, int segments)
    {
        Mesh mesh = new Mesh();

        int verticesCount = (segments + 1) * 2; // Inner and outer vertices for each segment
        Vector3[] vertices = new Vector3[verticesCount];
        int[] triangles = new int[segments * 6];

        float angleStep = angle / segments * Mathf.Deg2Rad;

        // Adjust rotation to align forwardDirection with Unity's default trig direction (right)
        Vector2 normalizedForward = forwardDirection.normalized;
        float forwardAngle = Mathf.Atan2(normalizedForward.y, normalizedForward.x);

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = forwardAngle - angle * Mathf.Deg2Rad / 2 + i * angleStep;

            // Calculate direction using the adjusted angle
            Vector3 direction = new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0);

            // Inner and outer vertices
            vertices[i] = direction * innerRadius;
            vertices[segments + 1 + i] = direction * outerRadius;

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

        return mesh;
    }


    private Mesh GenerateSectorBorderMesh(float innerRadius, float outerRadius, float angle, float borderThickness, int segments)
    {
        Mesh mesh = new Mesh();

        float extendedInnerRadius = innerRadius - borderThickness;
        float extendedOuterRadius = outerRadius + borderThickness;

        Vector3[] vertices = new Vector3[(segments + 1) * 4];
        int[] triangles = new int[segments * 12];

        float angleStep = angle / segments * Mathf.Deg2Rad;

        Vector2 normalizedForward = forwardDirection.normalized;
        float forwardAngle = Mathf.Atan2(normalizedForward.y, normalizedForward.x);

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = forwardAngle - angle * Mathf.Deg2Rad / 2 + i * angleStep;

            Vector3 direction = new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0);

            // Outer border vertices
            vertices[i] = direction * extendedOuterRadius;

            // Outer circle vertices
            vertices[segments + 1 + i] = direction * outerRadius;

            // Inner circle vertices
            vertices[(segments + 1) * 2 + i] = direction * innerRadius;

            // Inner border vertices
            vertices[(segments + 1) * 3 + i] = direction * extendedInnerRadius;

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

        return mesh;
    }


}
