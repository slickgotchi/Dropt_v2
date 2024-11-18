using UnityEngine;
using System.Collections.Generic;

public class CodeInjectorCanvas : DroptCanvas
{
    public static CodeInjectorCanvas Instance { get; private set; }

    [SerializeField] private List<CodeInjectorVariableItem> m_variableItemList;
    [SerializeField] private OutputMultiplierItem m_outputMultiplierItem;

    [HideInInspector] public Interactable interactable;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InstaHideCanvas();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
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
        CodeInjectorCanvas.Instance.HideCanvas();
        if (interactable != null)
        {
            PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactable.interactionText,
                interactable.interactableType);
        }
    }

    public void ClickOnReset()
    {
        CodeInjector.Instance.ResetUpdatedVariablesValueToDefalut();
        UpdateVariables();
    }
}