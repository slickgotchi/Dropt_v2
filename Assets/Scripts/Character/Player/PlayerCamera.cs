using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private GameObject m_trackedObject;
    public Vector3 Offset = new Vector3(0, 0.5f, 0);

    [Header("Follow Settings")]
    [SerializeField] private float smoothTime = 0.3f; // Higher = more laggy, Lower = snappier
    [SerializeField] private float maxLagDistance = 2f; // Max distance camera can be behind

    private Vector3 velocity = Vector3.zero;

    [Header("Shake Settings")]
    private float shakeDuration = 0f;
    private float shakeAmplitude = 0f;

    private void Update()
    {
        if (m_trackedObject != null)
        {
            Vector3 targetPosition = m_trackedObject.transform.position + Offset;

            // Enforce max lag distance (if the camera falls behind too much, it hard-snaps)
            if (Vector3.Distance(transform.position, targetPosition) > maxLagDistance)
            {
                transform.position = targetPosition - (targetPosition - transform.position).normalized * maxLagDistance;
            }

            // Smoothly move towards the target
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

            // Apply camera shake
            if (shakeDuration > 0)
            {
                float shakeX = (Mathf.PerlinNoise(Time.time * 50f, 0) - 0.5f) * shakeAmplitude;
                float shakeY = (Mathf.PerlinNoise(0, Time.time * 50f) - 0.5f) * shakeAmplitude;
                transform.position += new Vector3(shakeX, shakeY, 0);
                shakeDuration -= Time.deltaTime;
            }

            // Keep Z position fixed
            transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
        }
    }

    public void SetTrackedObject(GameObject trackedObject)
    {
        m_trackedObject = trackedObject;
    }

    public void Shake(float amplitude, float duration)
    {
        shakeAmplitude = amplitude;
        shakeDuration = duration;
    }
}
