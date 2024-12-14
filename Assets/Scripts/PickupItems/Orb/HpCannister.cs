using UnityEngine;

public class HpCannister : MonoBehaviour
{
    [SerializeField] private int m_hpValue;

    public int GetValue()
    {
        return m_hpValue;
    }
}
