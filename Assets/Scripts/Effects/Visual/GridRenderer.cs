using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    public float cellSize = 1f;
    public int gridWidth = 10;  // Total number of cells horizontally
    public int gridHeight = 10; // Total number of cells vertically
    public Color gridColor = Color.white;
    public float lineWidth = 0.1f;
    public string sortingLayerName = "Ground"; // Name of the sorting layer
    public int sortingOrder = 0;                // Order in the sorting layer

    void Start()
    {
        // Total lines needed: (gridWidth + 1) vertical lines + (gridHeight + 1) horizontal lines
        int totalLines = (gridWidth + 1) + (gridHeight + 1);
        for (int i = 0; i < totalLines; i++)
        {
            GameObject lineObj = new GameObject("GridLine" + i);
            lineObj.transform.SetParent(transform);
            LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = 2;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = gridColor;
            lineRenderer.endColor = gridColor;

            // Set the sorting layer and order
            lineRenderer.sortingLayerName = sortingLayerName;
            lineRenderer.sortingOrder = sortingOrder;

            if (i <= gridWidth)
            {
                // Vertical lines
                float xPos = (i - gridWidth / 2) * cellSize;
                lineRenderer.SetPosition(0, new Vector3(xPos, -(gridHeight / 2) * cellSize, 0));
                lineRenderer.SetPosition(1, new Vector3(xPos, (gridHeight / 2) * cellSize, 0));
            }
            else
            {
                // Horizontal lines
                float yPos = ((i - gridWidth - 1) - gridHeight / 2) * cellSize;
                lineRenderer.SetPosition(0, new Vector3(-(gridWidth / 2) * cellSize, yPos, 0));
                lineRenderer.SetPosition(1, new Vector3((gridWidth / 2) * cellSize, yPos, 0));
            }
        }
    }
}
