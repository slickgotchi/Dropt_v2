using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SunkenFloorButtonGroup : NetworkBehaviour
{
    [Header("State")]
    //public NetworkVariable<SunkenFloorType> Type;
    //public NetworkVariable<SunkenFloorState> State;
    //public int NumberButtons = 2;
    public NetworkVariable<int> NumberButtons = new NetworkVariable<int>();
    public int initNumberButtons = 2;
    public int spawnerId = -1;

    [HideInInspector] public List<GameObject> SunkenFloors = new List<GameObject>();

    //public void ButtonPressedDown()
    //{
    //    if (!IsServer) return;

    //    //var no_buttons = new List<SunkenFloorButton>(GetComponentsInChildren<SunkenFloorButton>());
    //    // get all buttons
    //    var no_allButtons = FindObjectsByType<SunkenFloorButton>
    //        (FindObjectsInactive.Include, FindObjectsSortMode.None);
    //    var no_buttons = new List<SunkenFloorButton>();
    //    for (int i = 0; i < no_allButtons.Length; i++)
    //    {
    //        if (spawnerId == no_allButtons[i].spawnerId)
    //        {
    //            no_buttons.Add(no_allButtons[i]);
    //        }
    //    }


    //    // count our down buttons
    //    int pressedDownCount = 0;
    //    foreach (var no_button in no_buttons)
    //    {
    //        if (no_button.State.Value != ButtonState.Up) pressedDownCount++;
    //    }

    //    // if all our buttons are pressed, raise the floor and lock the buttons
    //    if (pressedDownCount >= NumberButtons)
    //    {
    //        // raise all sunken floors
    //        foreach (var sunkenFloor in SunkenFloors)
    //        {
    //            sunkenFloor.GetComponent<SunkenFloor>().Raise();
    //        }

    //        // set all buttons to down locked
    //        foreach (var no_button in no_buttons)
    //        {
    //            no_button.State.Value = ButtonState.DownLocked;
    //        }
    //    }

    //    // ask the level parent to pop up all other platform buttons except ours
    //    PopupAllOtherPlatformButtons();
    //}

    //private void PopupAllOtherPlatformButtons()
    //{
    //    var no_sunkenFloorButtonGroups = FindObjectsByType<SunkenFloorButtonGroup>(FindObjectsSortMode.None);
    //    foreach (var no_sfButtonGroup in no_sunkenFloorButtonGroups)
    //    {
    //        if (no_sfButtonGroup.GetComponent<NetworkObject>().NetworkObjectId != NetworkObjectId)
    //        {
    //            var no_buttons = no_sfButtonGroup.GetComponentsInChildren<SunkenFloorButton>();
    //            foreach (var no_button in no_buttons)
    //            {
    //                if (no_button.State.Value != ButtonState.DownLocked)
    //                {
    //                    no_button.State.Value = ButtonState.Up;
    //                }
    //            }
    //        }
    //    }
    //}
}
