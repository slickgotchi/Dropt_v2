using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class CodeInjectorCanvas : DroptCanvas
{
    public static CodeInjectorCanvas Instance { get; private set; }

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
        PlayerInputMapSwitcher.Instance.SwitchToInUI();
    }

    public override void OnHideCanvas()
    {
        PlayerInputMapSwitcher.Instance.SwitchToInGame();
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
    }

    public void ClickOnReset()
    {
        CodeInjector.Instance.ResetUpdatedVariablesValueToDefalut();
        UpdateVariables();
    }
}