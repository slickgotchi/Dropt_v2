using UnityEngine;

public class FudWispExplosion : MonoBehaviour
{
    private float lifetime = 0.5f; // Example duration before returning to the pool

    private Animator m_animator;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        m_animator.StopPlayback();
        m_animator.Play("SplashExplosion");
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
