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
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

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