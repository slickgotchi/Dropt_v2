using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkLevel : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        CreateSunkenFloors();
        CreateApeDoors();
    }

    void CreateSunkenFloors()
    {
        // search for sunken floors spawners and button spawners
        var sunkenFloorSpawners = new List<SunkenFloor3x3Spawner>(GetComponentsInChildren<SunkenFloor3x3Spawner>());
        for (int i = 0; i < sunkenFloorSpawners.Count; i++)
        {
            var sunkenFloorSpawner = sunkenFloorSpawners[i];

            // insta/spawn network sunken floor and set its parent to the level
            var no_sunkenFloor = Instantiate(sunkenFloorSpawner.SunkenFloor3x3Prefab);
            no_sunkenFloor.transform.position = sunkenFloorSpawner.transform.position;
            no_sunkenFloor.GetComponent<NetworkObject>().Spawn();
            no_sunkenFloor.transform.parent = transform;

            // set the floor type
            no_sunkenFloor.GetComponent<SunkenFloor>().SetTypeAndSprite(sunkenFloorSpawner.SunkenFloorType);

            // get list of button spawners
            var buttonSpawners = new List<SunkenFloorButtonSpawner>(sunkenFloorSpawner.gameObject.GetComponentsInChildren<SunkenFloorButtonSpawner>());

            // randomly delete button spawners till we are at the required button count
            for (int j = 0; j < buttonSpawners.Count; j++)
            {
                if (buttonSpawners.Count > sunkenFloorSpawner.NumberButtons)
                {
                    var randIndex = UnityEngine.Random.Range(0, buttonSpawners.Count);
                    Destroy(buttonSpawners[randIndex].gameObject);
                    buttonSpawners.RemoveAt(randIndex);
                }
                else
                {
                    break;
                }
            }

            // in case the user specified more NumberButtons then we have button spawners, reduce the NumberButtons
            no_sunkenFloor.GetComponent<SunkenFloor>().NumberButtons = buttonSpawners.Count;

            // spawn children buttons
            for (int j = 0; j < buttonSpawners.Count; j++)
            {
                var buttonSpawner = buttonSpawners[j];

                // insta/spawn network buttno
                var no_Button = Instantiate(sunkenFloorSpawner.SunkenFloorButtonPrefab);
                no_Button.transform.position = buttonSpawner.transform.position;
                no_Button.GetComponent<NetworkObject>().Spawn();
                no_Button.GetComponent<SunkenFloorButton>().SetTypeStateAndSprite(sunkenFloorSpawner.SunkenFloorType, ButtonState.Up);

                // set network object buttons parent to the sunken floor
                no_Button.transform.parent = no_sunkenFloor.transform;
            }

            // destroy all button spawners
            while (buttonSpawners.Count > 0)
            {
                // destroy the spawner button
                Destroy(buttonSpawners[0].gameObject);
                buttonSpawners.RemoveAt(0);
            }

            // destroy button list
            buttonSpawners.Clear();
        }

        // Destroy all sunken floors
        while (sunkenFloorSpawners.Count > 0)
        {
            // destroy the spawner button
            Destroy(sunkenFloorSpawners[0].gameObject);
            sunkenFloorSpawners.RemoveAt(0);
        }

        // destroy list
        sunkenFloorSpawners.Clear();
    }

    public void PopupAllPlatformButtonsExcept(ulong excludeNetworkObjectId)
    {
        var no_sunkenFloors = GetComponentsInChildren<SunkenFloor>();
        foreach (var no_sunkenFloor in  no_sunkenFloors)
        {
            if (no_sunkenFloor.GetComponent<NetworkObject>().NetworkObjectId != excludeNetworkObjectId)
            {
                var no_buttons = no_sunkenFloor.GetComponentsInChildren<SunkenFloorButton>();
                foreach (var no_button in no_buttons)
                {
                    if (no_button.ButtonState != ButtonState.DownLocked)
                    {
                        no_button.SetTypeStateAndSprite(no_sunkenFloor.SunkenFloorType, ButtonState.Up);
                    }
                }
            }
        }
    }

    void CreateApeDoors()
    {
        // search for sunken floors spawners and button spawners
        var apeDoorSpawners = new List<ApeDoorSpawner>(GetComponentsInChildren<ApeDoorSpawner>());
        for (int i = 0; i < apeDoorSpawners.Count; i++)
        {
            var apeDoorSpawner = apeDoorSpawners[i];

            // insta/spawn network sunken floor and set its parent to the level
            var no_apeDoor = Instantiate(apeDoorSpawner.ApeDoorPrefab);
            no_apeDoor.transform.position = apeDoorSpawner.transform.position;
            no_apeDoor.GetComponent<NetworkObject>().Spawn();
            no_apeDoor.transform.parent = transform;

            // set the floor type
            no_apeDoor.GetComponent<ApeDoor>().SetTypeAndSprite(apeDoorSpawner.ApeDoorType);

            // get list of button spawners
            var buttonSpawners = new List<ApeDoorButtonSpawner>(apeDoorSpawner.gameObject.GetComponentsInChildren<ApeDoorButtonSpawner>());

            // randomly delete button spawners till we are at the required button count
            for (int j = 0; j < buttonSpawners.Count; j++)
            {
                if (buttonSpawners.Count > apeDoorSpawner.NumberButtons)
                {
                    var randIndex = UnityEngine.Random.Range(0, buttonSpawners.Count);
                    Destroy(buttonSpawners[randIndex].gameObject);
                    buttonSpawners.RemoveAt(randIndex);
                }
                else
                {
                    break;
                }
            }

            // in case the user specified more NumberButtons then we have button spawners, reduce the NumberButtons
            no_apeDoor.GetComponent<ApeDoor>().NumberButtons = buttonSpawners.Count;

            // spawn children buttons
            for (int j = 0; j < buttonSpawners.Count; j++)
            {
                var buttonSpawner = buttonSpawners[j];

                // insta/spawn network buttno
                var no_Button = Instantiate(apeDoorSpawner.ApeDoorButtonPrefab);
                no_Button.transform.position = buttonSpawner.transform.position;
                no_Button.GetComponent<NetworkObject>().Spawn();
                no_Button.GetComponent<ApeDoorButton>().SetTypeStateAndSprite(apeDoorSpawner.ApeDoorType, ButtonState.Up);

                // set network object buttons parent to the sunken floor
                no_Button.transform.parent = no_apeDoor.transform;
            }

            // destroy all button spawners
            while (buttonSpawners.Count > 0)
            {
                // destroy the spawner button
                Destroy(buttonSpawners[0].gameObject);
                buttonSpawners.RemoveAt(0);
            }

            // destroy button list
            buttonSpawners.Clear();
        }

        // Destroy all sunken floors
        while (apeDoorSpawners.Count > 0)
        {
            // destroy the spawner button
            Destroy(apeDoorSpawners[0].gameObject);
            apeDoorSpawners.RemoveAt(0);
        }

        // destroy list
        apeDoorSpawners.Clear();
    }

    public void PopupAllApeDoorButtonsExcept(ulong excludeNetworkObjectId)
    {
        var no_apeDoors = GetComponentsInChildren<ApeDoor>();
        foreach (var no_apeDoor in no_apeDoors)
        {
            if (no_apeDoor.GetComponent<NetworkObject>().NetworkObjectId != excludeNetworkObjectId)
            {
                var no_buttons = no_apeDoor.GetComponentsInChildren<ApeDoorButton>();
                foreach (var no_button in no_buttons)
                {
                    if (no_button.ButtonState != ButtonState.DownLocked)
                    {
                        no_button.SetTypeStateAndSprite(no_apeDoor.ApeDoorType, ButtonState.Up);
                    }
                }
            }
        }
    }
}


