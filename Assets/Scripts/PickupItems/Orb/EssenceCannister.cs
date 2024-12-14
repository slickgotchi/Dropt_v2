using UnityEngine;

public class EssenceCannister : MonoBehaviour
{
    [SerializeField] private int m_essenceValue;

    public int GetValue()
    {
        return m_essenceValue;
    }
}
