using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class CodeInjectorCanvas : MonoBehaviour
{
    public static CodeInjectorCanvas Instance { get; private set; }

    [SerializeField] private GameObject m_container;
    [SerializeField] private List<CodeInjectorVariableItem> m_variableItemList;
    [SerializeField] private OutputMultiplierItem m_outputMultiplierItem;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        m_container.SetActive(false);
    }

    public void SetVisible(bool isVisible)
    {
        m_container.SetActive(isVisible);
    }

    public void UpdateUI()
    {
        foreach (CodeInjectorVariableItem item in m_variableItemList)
        {
            item.Initialize();
        }
        m_outputMultiplierItem.Initialize();
    }

    public void ClickOnConfirm()
    {
        CodeInjector.Instance.UpdateVariablesData();
    }

    public void ClickOnReset()
    {
        CodeInjector.Instance.ResetAllVariable();
        UpdateUI();
    }
}
