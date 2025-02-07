using UnityEngine;

public class VFX_DeathBloodSkull : MonoBehaviour
{
    private float lifetime = 1f; // Example duration before returning to the pool

    private void OnEnable()
    {
        Invoke("ReturnToPool", lifetime);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void ReturnToPool()
    {
        VisualEffectsManager.Instance.ReturnToPool(gameObject);
    }
}

