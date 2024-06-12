using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

[InitializeOnLoad]
public static class TilemapSelector
{
    static TilemapSelector()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        Debug.Log("TilemapSelector initialized.");
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        // Check for a mouse down event with the left mouse button and Control key pressed
        if (e.type == EventType.MouseDown && e.button == 0 && (e.modifiers & EventModifiers.Control) != 0)
        {
            Debug.Log("Control-click detected.");

            // Cast a ray from the mouse position
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null)
            {
                Debug.Log($"Hit detected on: {hit.collider.gameObject.name}");
                Tilemap tilemap = hit.collider.GetComponent<Tilemap>();
                if (tilemap != null)
                {
                    Selection.activeGameObject = tilemap.gameObject;
                    Debug.Log($"Tilemap selected: {tilemap.gameObject.name}");
                    e.Use();  // Mark the event as used to prevent further processing
                }
                else
                {
                    Debug.Log("No Tilemap component found on the hit object.");
                }
            }
            else
            {
                Debug.Log("No hit detected.");
            }
        }
    }
}
