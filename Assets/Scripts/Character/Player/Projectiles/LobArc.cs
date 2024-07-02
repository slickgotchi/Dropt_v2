using Unity.Mathematics;
using UnityEngine;

public class LobArc : MonoBehaviour
{
    public float MaxHeight = 1f;
    public float Duration_s = 0.5f;
    private float timer_s = 0f;

    public GameObject Body;

    public void Reset()
    {
        timer_s = 0f;
    }

    private void Update()
    {
        timer_s += Time.deltaTime;
        if (timer_s > Duration_s) timer_s = Duration_s;

        float alpha = timer_s / Duration_s;
        float y = math.sin(alpha * math.PI) * MaxHeight;

        Vector3 pos = Body.transform.localPosition;
        pos.y = y;
        Body.transform.localPosition = pos;
    }
}
