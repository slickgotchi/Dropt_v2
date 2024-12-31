using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GeneralDebugCanvas : DroptCanvas
{
    public static GeneralDebugCanvas Instance { get; private set; }

    public Toggle fpsToggle;
    public Toggle characterStatsToggle;
    public Toggle enemyAIToggle;

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
        if (Game.Instance == null) return;

        var playerControllers = Game.Instance.playerControllers;

        foreach (var pc in playerControllers)
        {
            var canvas = pc.GetComponentInChildren<NetworkCharacterDebugCanvas>();
            canvas.Container.SetActive(visible);
        }
    }

    private void SetEnemyStateVisible(bool visible)
    {
        //if (Game.Instance == null) return;



        //var enemyControllers = Game.Instance.enemyControllers;

        //foreach (var ec in enemyControllers)
        //{
        //    bool isActive = ec.gameObject.activeSelf;
        //    ec.gameObject.SetActive(true);
        //    var canvas = ec.GetComponentInChildren<EnemyAI_DebugCanvas>();
        //    canvas.Container.SetActive(visible);
        //    ec.gameObject.SetActive(isActive);
        //}
    }
}
