using UnityEngine;

public class CloudExplosion : MonoBehaviour
{
    private float lifetime = 0.5f; // Example duration before returning to the pool

    private Animator m_animator;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        m_animator.Play("CloudExplosion");
        Invoke("ReturnToPool", lifetime);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void ReturnToPool()
    {
        transform.localScale = Vector3.one;
        VisualEffectsManager.Instance.ReturnToPool(gameObject);
    }
}
