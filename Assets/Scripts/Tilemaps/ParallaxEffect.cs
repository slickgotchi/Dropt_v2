using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    public Transform cameraTransform; // Reference to the camera's transform
    public float parallaxFactor = 0.5f; // Adjust this to control how much the layer moves
    private Vector3 previousCameraPosition;

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform; // Ensure camera is assigned
        }

        // Store the initial camera position
        previousCameraPosition = cameraTransform.position;
    }

    void Update()
    {
        // Calculate the difference in the camera's position since the last frame
        Vector3 deltaMovement = cameraTransform.position - previousCameraPosition;

        // Apply parallax movement to the tilemap layer
        transform.position += deltaMovement * parallaxFactor;

        // Update the previous camera position
        previousCameraPosition = cameraTransform.position;
    }
}
