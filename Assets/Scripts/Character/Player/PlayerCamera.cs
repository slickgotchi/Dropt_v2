using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private GameObject m_trackedObject;
    public Vector3 Offset = new Vector3(0, 0.5f, 0);

    [Header("Follow Settings")]
    [SerializeField] private float smoothTime = 0.175f; // Normal smooth time
    [SerializeField] private float shakeSmoothTime = 0.05f; // Faster response when shaking
    [SerializeField] private float maxLagDistance = 20f; // Max camera lag distance

    private Vector3 velocity = Vector3.zero;

    [Header("Shake Settings")]
    private float shakeDuration = 0f;
    private float shakeAmplitude = 0f;
    private Vector3 shakeOffset = Vector3.zero;

    private void Update()
    {
        if (m_trackedObject != null)
        {
            Vector3 targetPosition = m_trackedObject.transform.position + Offset;

            // Enforce max lag distance (prevent infinite lagging)
            if (Vector3.Distance(transform.position, targetPosition) > maxLagDistance)
            {
                transform.position = targetPosition - (targetPosition - transform.position).normalized * maxLagDistance;
            }

            // **Temporarily Reduce Smoothing During Shake**
            float currentSmoothTime = (shakeDuration > 0) ? shakeSmoothTime : smoothTime;

            // Smoothly move towards the target
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, currentSmoothTime);

            // Apply shake effect
            if (shakeDuration > 0)
            {
                shakeOffset = new Vector3(
                    (Mathf.PerlinNoise(Time.time * 50f, 0) - 0.5f) * shakeAmplitude,
                    (Mathf.PerlinNoise(0, Time.time * 50f) - 0.5f) * shakeAmplitude,
                    0
                );
                shakeDuration -= Time.deltaTime;
            }
            else
            {
                shakeOffset = Vector3.zero; // Reset shake after duration ends
            }

            // Apply final position with shake
            transform.position = new Vector3(
                smoothedPosition.x + shakeOffset.x,
                smoothedPosition.y + shakeOffset.y,
                -10f // Keep Z locked
            );
        }
    }

    public void SetTrackedObject(GameObject trackedObject)
    {
        m_trackedObject = trackedObject;
    }

    public void Shake(float amplitude, float duration)
    {
        Debug.Log("Do shake");
        shakeAmplitude = amplitude;
        shakeDuration = duration;
    }

    /// <summary>
    /// Instantly moves the camera to the tracked object's position without smoothing.
    /// </summary>
    public void SnapToTrackedObjectImmediately()
    {
        if (m_trackedObject == null) return;

        Vector3 targetPosition = m_trackedObject.transform.position + Offset;
        transform.position = new Vector3(targetPosition.x, targetPosition.y, -10f); // Keep Z fixed
        velocity = Vector3.zero; // Reset velocity to prevent SmoothDamp artifacts
    }
}
