using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerInputMapSwitcher : MonoBehaviour
{
    public static PlayerInputMapSwitcher Instance { get; private set; }

    private float m_switchTimer = 0f;

    private string actionMapToSwitch;   // Store the action map to switch to
    private bool shouldSwitchActionMap; // Flag to determine if we should switch in the next frame

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // Check if we should switch the action map in this frame
        if (shouldSwitchActionMap)
        {
            PerformSwitch();  // Perform the actual switch
            shouldSwitchActionMap = false;  // Reset the flag so we only switch once
        }

        m_switchTimer -= Time.deltaTime;
    }

    public bool IsSwitchTooRecent()
    {
        return m_switchTimer > 0;
    }

    public void SwitchToInGame()
    {
        ScheduleActionMapSwitch("InGame");
    }

    public void SwitchToInUI()
    {
        ScheduleActionMapSwitch("InUI");
    }

    private void ScheduleActionMapSwitch(string actionMap)
    {
        // Set the action map to switch to and mark the switch to happen in the next frame
        actionMapToSwitch = actionMap;
        shouldSwitchActionMap = true;
    }

    private void PerformSwitch()
    {
        // Perform the actual switch to the scheduled action map
        var playerControllers = Game.Instance.playerControllers;

        foreach (var pc in playerControllers)
        {
            var networkObject = pc.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsLocalPlayer)
            {
                pc.GetComponent<PlayerInput>().SwitchCurrentActionMap(actionMapToSwitch);
                m_switchTimer = 0.5f;
            }
        }
    }
}
