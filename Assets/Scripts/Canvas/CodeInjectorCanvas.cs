using UnityEngine;
using System.Collections.Generic;
using System;

public class CodeInjectorCanvas : MonoBehaviour
{
    public static CodeInjectorCanvas Instance { get; private set; }

    [SerializeField] private GameObject m_container;
    [SerializeField] private List<CodeInjectorVariableItem> m_variableItemList;

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

    public void InitializeVariableValues()
    {
        foreach (CodeInjectorVariableItem item in m_variableItemList)
        {
            item.Initialize();
        }
    }

    public void ClickOnConfirm()
    {
        CodeInjector.Instance.UpdateVariablesData();
    }

    public void ClickOnReset()
    {
        foreach (var item in m_variableItemList)
        {
            item.Reset();
        }
    }

    public void UpdateVariableText(CodeInjector.Variable type)
    {

    }
}
