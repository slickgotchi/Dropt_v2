using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GeneralDebugCanvas : DroptCanvas
{
    public Toggle fpsToggle;
    public Toggle characterStatsToggle;
    public Toggle enemyAIToggle;
    //public Toggle consoleLogToggle;

    private void Awake()
    {
        InstaHideCanvas();

        if (fpsToggle != null)
        {
            fpsToggle.onValueChanged.AddListener(SetFPSVisible);
            SetFPSVisible(fpsToggle.isOn);
        }

        // Set the initial visibility based on the toggle and subscribe to changes
        if (characterStatsToggle != null)
        {
            characterStatsToggle.onValueChanged.AddListener(SetCharacterStatsVisibile);
            SetCharacterStatsVisibile(characterStatsToggle.isOn); // Ensure initial state is applied
        }

        if (enemyAIToggle != null)
        {
            enemyAIToggle.onValueChanged.AddListener(SetEnemyStateVisible);
            SetEnemyStateVisible(enemyAIToggle.isOn);
        }

        //if (consoleLogToggle != null)
        //{
        //    consoleLogToggle.onValueChanged.AddListener(SetConsoleLogVisible);
        //    SetConsoleLogVisible(consoleLogToggle.isOn);
        //}
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (PlayerEquipmentDebugCanvas.Instance.isCanvasOpen)
            {
                HideCanvas();
            }
            else
            {
                ShowCanvas();
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the toggle's event to avoid memory leaks
        if (characterStatsToggle != null)
        {
            characterStatsToggle.onValueChanged.RemoveListener(SetCharacterStatsVisibile);
        }
    }

    private void SetFPSVisible(bool visible)
    {
        DebugCanvas.Instance.Container.SetActive(visible);
    }

    private void SetCharacterStatsVisibile(bool visible)
    {
        foreach (var canvas in FindObjectsByType<NetworkCharacterDebugCanvas>(FindObjectsSortMode.None))
        {
            canvas.Container.SetActive(visible);
        }
    }

    private void SetEnemyStateVisible(bool visible)
    {
        var debugCanvases = FindObjectsByType<EnemyAI_DebugCanvas>(FindObjectsSortMode.None);
        foreach (var dc in debugCanvases)
        {
            dc.Container.SetActive(visible);
        }
    }

    //private void SetConsoleLogVisible(bool visible)
    //{
    //    DebugLogDisplay.Instance.container.SetActive(visible);
    //}
}
