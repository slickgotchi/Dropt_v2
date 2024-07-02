using UnityEngine;

public class DynamicSorting : MonoBehaviour
{
    public GameObject targetToTrack;
    public int sortingOrderOffset = 0;

    private SpriteRenderer m_spriteRenderer;
    private int k_multiplicationFactor = 10;

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_spriteRenderer.sortingLayerID = SortingLayer.NameToID("Dynamic");
    }

    void Update()
    {
        if (targetToTrack != null)
        {
            m_spriteRenderer.sortingOrder = (int)(k_multiplicationFactor * -targetToTrack.transform.position.y) + sortingOrderOffset;
        } else
        {
            m_spriteRenderer.sortingOrder = (int)(k_multiplicationFactor * -transform.position.y) + sortingOrderOffset;
        }
    }
}
