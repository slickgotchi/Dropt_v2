using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

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
        SetVisible(false);
        StartPlayerMovement();
    }

    public void ClickOnReset()
    {
        CodeInjector.Instance.ResetUpdatedVariablesValueToDefalut();
        UpdateVariables();
    }

    public void StartPlayerMovement()
    {
        var players = FindObjectsByType<PlayerPrediction>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.GetComponent<NetworkObject>().IsLocalPlayer)
            {
                player.IsInputEnabled = true;
            }
        }
    }
}