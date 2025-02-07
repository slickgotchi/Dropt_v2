using UnityEngine;

public class DynamicSorting : MonoBehaviour
{
    public GameObject targetToTrack;
    public int sortingOrderOffset = 0;

    private SpriteRenderer m_spriteRenderer;
    private int k_multiplicationFactor = 10;

    private ParticleSystem m_particleSystem;
    private ParticleSystemRenderer m_particleSystemRenderer;

    int m_sortingOrder = 0;

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        if (m_spriteRenderer != null)
        {
            m_spriteRenderer.sortingLayerID = SortingLayer.NameToID("Dynamic");
        }

        m_particleSystem = GetComponent<ParticleSystem>();
        if (m_particleSystem != null)
        {
            m_particleSystemRenderer = m_particleSystem.GetComponent<ParticleSystemRenderer>();
            m_particleSystemRenderer.sortingLayerID = SortingLayer.NameToID("Dynamic");
        }
    }

    void Update()
    {
        if (targetToTrack != null)
        {
            m_sortingOrder = (int)(k_multiplicationFactor * -targetToTrack.transform.position.y) + sortingOrderOffset;
        } else
        {
            m_sortingOrder = (int)(k_multiplicationFactor * -transform.position.y) + sortingOrderOffset;
        }

        if (m_spriteRenderer != null)
        {
            m_spriteRenderer.sortingOrder = m_sortingOrder;
        }

        if (m_particleSystem != null)
        {
            m_particleSystemRenderer.sortingOrder = m_sortingOrder;
        }
    }
}
