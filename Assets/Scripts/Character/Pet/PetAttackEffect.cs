using UnityEngine;

public class PetAttackEffect : MonoBehaviour
{
    private void OnEnable()
    {
        GetComponent<ParticleSystem>().Play();
    }

    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(Despawn), 1f);
    }

    private void Despawn()
    {
        gameObject.SetActive(false);
        VisualEffectsManager.Singleton.ReturnToPool(gameObject);
    }
}
