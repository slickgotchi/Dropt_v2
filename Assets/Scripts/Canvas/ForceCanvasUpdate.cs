using UnityEngine;
using UnityEngine.UI;

public class ForceCanvasUpdate : MonoBehaviour
{
    private CanvasScaler canvasScaler;
    private bool initialized = false;

    void Start()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        if (canvasScaler != null)
        {
            // Force update immediately on start
            ForceUpdate();
        }
    }

    void Update()
    {
        // Ensure this runs only once after the initial frame
        if (!initialized)
        {
            initialized = true;
            ForceUpdate();
        }
    }

    void ForceUpdate()
    {
        // Change the reference resolution slightly and then revert to force the canvas to update
        Vector2 originalResolution = canvasScaler.referenceResolution;
        canvasScaler.referenceResolution = new Vector2(originalResolution.x + 1, originalResolution.y + 1);
        canvasScaler.referenceResolution = originalResolution;

        // Rebuild the layout
        Canvas.ForceUpdateCanvases();
    }
}
