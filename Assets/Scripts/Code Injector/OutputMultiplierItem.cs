using UnityEngine;
using TMPro;

public class OutputMultiplierItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_valueText;

    public void Initialize()
    {
        m_valueText.text = CodeInjector.Instance.GetOutputMultiplier().ToString("F2");
    }
}
