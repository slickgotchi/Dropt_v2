using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AspectRatioMaintainer : MonoBehaviour
{
    public Vector2 targetAspectRatio = new Vector2(16, 10);  // Target aspect ratio of 16:10

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        UpdateCameraViewport();
    }

    void Update()
    {
        UpdateCameraViewport();  // Adjust the viewport in real-time (optional, for dynamic resizing)
    }

    void UpdateCameraViewport()
    {
        float targetAspect = targetAspectRatio.x / targetAspectRatio.y;
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f) // Letterbox (add black bars on top and bottom)
        {
            Rect rect = cam.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            cam.rect = rect;
        }
        else // Pillarbox (add black bars on the sides)
        {
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = cam.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            cam.rect = rect;
        }
    }
}
