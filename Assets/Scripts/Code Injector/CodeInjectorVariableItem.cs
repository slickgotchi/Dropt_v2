using UnityEngine;
using TMPro;

public class CodeInjectorVariableItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_valueText;
    [SerializeField] private CodeInjector.Variable m_variableType;

    public void Initialize()
    {
        m_valueText.text = CodeInjector.Instance.GetVariableString(m_variableType);
    }

    public void Add()
    {
        CodeInjector.Instance.AddVariable(m_variableType);
        m_valueText.text = CodeInjector.Instance.GetVariableString(m_variableType);
    }

    public void Subtract()
    {
        CodeInjector.Instance.SubtractVariable(m_variableType);
        m_valueText.text = CodeInjector.Instance.GetVariableString(m_variableType);
    }
}
