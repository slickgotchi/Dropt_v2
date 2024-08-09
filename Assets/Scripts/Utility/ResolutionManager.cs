using UnityEngine;

public class AspectRatioManager : MonoBehaviour
{
    public Vector2 targetAspectRatio = new Vector2(16, 10); // Set your desired aspect ratio, e.g., 16:10

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        ApplyAspectRatio();
    }

    private void Update()
    {
        // Optionally, you can check for window size changes and reapply the aspect ratio if needed
        if (Input.GetKeyDown(KeyCode.T))
        {
            ApplyAspectRatio();
        }
    }

    void ApplyAspectRatio()
    {
        // Calculate the target aspect ratio
        float targetAspect = targetAspectRatio.x / targetAspectRatio.y;
        // Get the current screen aspect ratio
        float windowAspect = (float)Screen.width / Screen.height;
        // Determine the scaling needed to maintain the aspect ratio
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f) // Letterbox: Black bars on top and bottom
        {
            float adjustedHeight = scaleHeight;
            cam.rect = new Rect(0, (1.0f - adjustedHeight) / 2.0f, 1.0f, adjustedHeight);
        }
        else // Pillarbox: Black bars on the sides
        {
            float scaleWidth = 1.0f / scaleHeight;
            cam.rect = new Rect((1.0f - scaleWidth) / 2.0f, 0, scaleWidth, 1.0f);
        }

        // Clear the rest of the screen to black to cover the letterbox/pillarbox areas
        GL.Clear(true, true, Color.black);
    }
}
