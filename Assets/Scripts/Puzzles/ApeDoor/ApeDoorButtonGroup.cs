using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ApeDoorButtonGroup : NetworkBehaviour
{
    [Header("State")]
    public NetworkVariable<ApeDoorType> Type;
    public NetworkVariable<DoorState> State;
    public int NumberButtons = 2;
    public int spawnerId = -1;

    [HideInInspector] public List<GameObject> ApeDoors = new List<GameObject>();

    public void ButtonPressedDown()
    {
        if (!IsServer) return;

        //var no_buttons = new List<ApeDoorButton>(GetComponentsInChildren<ApeDoorButton>());
        var no_allButtons = FindObjectsByType<ApeDoorButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var no_buttons = new List<ApeDoorButton>();
        for (int i = 0; i < no_allButtons.Length; i++)
        {
            if (spawnerId == no_allButtons[i].spawnerId)
            {
                Debug.Log("found matching button for the group");
                no_buttons.Add(no_allButtons[i]);
            }
        }

        // count our down buttons
        int pressedDownCount = 0;
        foreach (var no_button in no_buttons)
        {
            if (no_button.State.Value != ButtonState.Up) pressedDownCount++;
        }

        // if all our buttons are pressed, raise the floor and lock the buttons
        if (pressedDownCount >= NumberButtons)
        {
            // open all ape doors
            foreach (var apeDoor in ApeDoors)
            {
                Debug.Log("Open door");
                apeDoor.GetComponent<ApeDoor>().Open();
            }

            // set all buttons to down locked
            foreach (var no_button in no_buttons)
            {
                no_button.State.Value = ButtonState.DownLocked;
            }
        }

        // pop up all other buttons except ours
        PopupAllOtherDoorButtons();
    }

    private void PopupAllOtherDoorButtons()
    {
        var no_apeDoorButtonGroups = FindObjectsByType<ApeDoorButtonGroup>(FindObjectsSortMode.None);
        foreach (var no_apeDoorButtonGroup in no_apeDoorButtonGroups)
        {
            if (no_apeDoorButtonGroup.GetComponent<NetworkObject>().NetworkObjectId != NetworkObjectId)
            {
                var no_buttons = no_apeDoorButtonGroup.GetComponentsInChildren<ApeDoorButton>();
                foreach (var no_button in no_buttons)
                {
                    if (no_button.State.Value != ButtonState.DownLocked)
                    {
                        no_button.State.Value = ButtonState.Up;
                    }
                }
            }
        }
    }
}
