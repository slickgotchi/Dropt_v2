using Unity.Mathematics;
using UnityEngine;

public class DamageWobble : MonoBehaviour
{

    public int NumberWobbles = 1;
    public float WobbleDuration = 0.3f;
    public float MaxWobbleAngle = 15f;

    private bool isPlaying = false; // Flag to indicate if the flashing is active
    private float timer_s = 0f;

    private GameObject m_wobbleBody;

    private void Awake()
    {
        m_wobbleBody = transform.Find("Body").gameObject;
    }

    void Update()
    {
        if (isPlaying)
        {
            timer_s += Time.deltaTime;

            float angle = MaxWobbleAngle * math.sin(timer_s * 2 * math.PI / WobbleDuration);
            m_wobbleBody.transform.rotation = Quaternion.Euler(0, angle, angle);

            if (timer_s > WobbleDuration * NumberWobbles)
            {
                isPlaying = false;
            }
        }
    }

    // Call this method to start flashing
    public void Play()
    {
        if (!isPlaying)
        {
            isPlaying = true;
            timer_s = 0;
        }
    }
}
