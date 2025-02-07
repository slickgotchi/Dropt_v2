using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AABBErrorFinder : EditorWindow
{
    [MenuItem("Tools/Find Invalid AABB Issues")]
    public static void FindInvalidAABB()
    {
        Debug.Log("Starting search for Invalid AABB issues...");

        // Iterate through all GameObjects in the active scene
        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
        {
            // Check for CanvasRenderer components
            CanvasRenderer canvasRenderer = obj.GetComponent<CanvasRenderer>();
            if (canvasRenderer != null)
            {
                // Check if the associated RectTransform is valid
                RectTransform rectTransform = obj.GetComponent<RectTransform>();
                if (rectTransform != null && HasInvalidRectTransform(rectTransform))
                {
                    Debug.LogError($"Invalid RectTransform found on GameObject: {obj.name}", obj);
                }

                // Check if CanvasRenderer has invalid state
                if (HasInvalidCanvasRenderer(canvasRenderer))
                {
                    Debug.LogError($"Invalid CanvasRenderer found on GameObject: {obj.name}", obj);
                }
            }

            // Check for UI Graphic components (e.g., Image, Text, RawImage)
            Graphic graphic = obj.GetComponent<Graphic>();
            if (graphic != null && HasInvalidGraphic(graphic))
            {
                Debug.LogError($"Invalid Graphic found on GameObject: {obj.name}", obj);
            }
        }

        Debug.Log("Search for Invalid AABB issues completed.");
    }

    private static bool HasInvalidRectTransform(RectTransform rectTransform)
    {
        // Check for NaN or extreme values in RectTransform
        if (float.IsNaN(rectTransform.rect.width) || float.IsNaN(rectTransform.rect.height) ||
            rectTransform.rect.width < 0 || rectTransform.rect.height < 0)
        {
            return true;
        }

        if (float.IsInfinity(rectTransform.rect.width) || float.IsInfinity(rectTransform.rect.height))
        {
            return true;
        }

        return false;
    }

    private static bool HasInvalidCanvasRenderer(CanvasRenderer canvasRenderer)
    {
        // CanvasRenderer does not directly expose a mesh anymore; we can only check for its state
        if (canvasRenderer.cullTransparentMesh && canvasRenderer.GetAlpha() <= 0f)
        {
            Debug.LogWarning($"CanvasRenderer on {canvasRenderer.gameObject.name} is potentially problematic due to culling with zero alpha.");
            return true;
        }

        return false;
    }

    private static bool HasInvalidGraphic(Graphic graphic)
    {
        // Check if the Graphic's rectTransform or CanvasRenderer is invalid
        RectTransform rectTransform = graphic.rectTransform;
        if (rectTransform != null && HasInvalidRectTransform(rectTransform))
        {
            return true;
        }

        return false;
    }
}
