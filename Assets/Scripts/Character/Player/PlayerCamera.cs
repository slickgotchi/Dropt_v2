using UnityEngine;
using Unity.Mathematics;


public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private GameObject m_trackedObject;
    public Vector3 Offset = new Vector3(0, 0.5f, 0);

    [Header("Follow Settings")]
    [SerializeField] private float m_smoothTime = 0.3f; // Normal smooth time
    [SerializeField] private float maxLagDistance = 20f; // Max camera lag distance

    private Vector3 velocity = Vector3.zero;

    [Header("Shake Settings")]
    private float m_shakeTimer_s = 0f;
    private float m_shakeAmplitude = 0f;
    private float m_shakeDuration_s = 0f;
    private float m_elapsedTime_s = 0f;
    private float m_shakeFrequency = 1f;

    private void Start()
    {
        m_shakeTimer_s = 0f;
        m_shakeAmplitude = 0f;
    }

    private void Update()
    {
        if (m_trackedObject == null) return;

        Vector3 targetPosition = m_trackedObject.transform.position + Offset;

        // Enforce max lag distance (prevent infinite lagging)
        if (Vector3.Distance(transform.position, targetPosition) > maxLagDistance)
        {
            transform.position = targetPosition - (targetPosition - transform.position).normalized * maxLagDistance;
        }

        // Perlin noise shake
        Vector2 shakeOffset = Vector2.zero;
        m_elapsedTime_s += Time.deltaTime;
        if (m_shakeTimer_s > 0)
        {
            float shakeAmount = m_shakeAmplitude * (m_shakeTimer_s / m_shakeDuration_s); // Fades out over time
            shakeOffset.x = noise.snoise(new float2(m_elapsedTime_s * m_shakeFrequency, 0)) * shakeAmount;
            shakeOffset.y = noise.snoise(new float2(0, m_elapsedTime_s * m_shakeFrequency)) * shakeAmount;

            m_shakeTimer_s -= Time.deltaTime;
        }

        // Smoothly move towards target position
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, m_smoothTime);

        // Apply final shake-adjusted position
        transform.position = new Vector3(smoothedPosition.x + shakeOffset.x, smoothedPosition.y + shakeOffset.y, -10f);
    }

    public void SetTrackedObject(GameObject trackedObject)
    {
        m_trackedObject = trackedObject;
    }

    /// <summary>
    /// Starts a camera shake effect.
    /// </summary>
    public void Shake(float amplitude = 0.05f, float duration = 0.2f, float frequency = 10)
    {
        m_shakeAmplitude = amplitude;
        m_shakeTimer_s = duration;
        m_shakeDuration_s = duration;
        m_shakeFrequency = frequency;
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
