using UnityEngine;
using System.Collections.Generic;

public class CodeInjectorCanvas : DroptCanvas
{
    public static CodeInjectorCanvas Instance { get; private set; }

    [SerializeField] private List<CodeInjectorVariableItem> m_variableItemList;
    [SerializeField] private OutputMultiplierItem m_outputMultiplierItem;

    public Interactable interactable;

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

        HideCanvas();
    }

    private void Update()
    {
        base.Update();

        if (IsInputActionSelectPressed())
        {
            ClickOnConfirm();
        }
    }

    public override void OnShowCanvas()
    {
        
    }

    public override void OnHideCanvas()
    {
        
    }

    public void UpdateVariables()
    {
        foreach (CodeInjectorVariableItem item in m_variableItemList)
        {
            item.Initialize();
        }
    }

    public void UpdateOutputMultiplier()
    {
        m_outputMultiplierItem.Initialize();
    }

    public void ClickOnConfirm()
    {
        CodeInjector.Instance.UpdateVariablesData();
        HideCanvas();
        if (interactable != null) interactable.ExternalCanvasClosed();
    }

    public void ClickOnReset()
    {
        CodeInjector.Instance.ResetUpdatedVariablesValueToDefalut();
        UpdateVariables();
    }
}