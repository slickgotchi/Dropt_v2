using UnityEngine;

public class FloatBob : MonoBehaviour
{
    // Public variables to be set in the Inspector
    public float NeutralHeightY = 0f;
    public float AmplitudeY = 1f;
    public float Frequency = 1f;

    // Internal variable to keep track of the initial local position
    private Vector3 initialLocalPosition;

    // Start is called before the first frame update
    void Start()
    {
        // Record the initial local position of the object
        initialLocalPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the new Y position using a sine wave
        float newY = NeutralHeightY + AmplitudeY * Mathf.Sin(Time.time * Frequency);

        // Apply the new local position to the object
        transform.localPosition = new Vector3(initialLocalPosition.x, initialLocalPosition.y + newY, initialLocalPosition.z);
    }
}
