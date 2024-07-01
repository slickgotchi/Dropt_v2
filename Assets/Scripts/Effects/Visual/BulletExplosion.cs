using UnityEngine;

public class BulletExplosion : MonoBehaviour
{
    private float lifetime = 0.5f; // Example duration before returning to the pool

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
        VisualEffectsManager.Singleton.ReturnToPool(gameObject);
    }
}
