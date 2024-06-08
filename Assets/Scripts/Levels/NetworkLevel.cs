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
    }

    void CreateSunkenFloors()
    {
        // search for sunken floors spawners and button spawners
        var sunkenFloorSpawners = new List<SunkenFloor3x3Spawner>(GetComponentsInChildren<SunkenFloor3x3Spawner>());
        for (int i = 0; i < sunkenFloorSpawners.Count; i++)
        {
            var sunkenFloorSpawner = sunkenFloorSpawners[i];

            // insta/spawn network sunken floor
            var no_SunkenFloor = Instantiate(sunkenFloorSpawner.SunkenFloor3x3Prefab);
            no_SunkenFloor.transform.position = sunkenFloorSpawner.transform.position;
            no_SunkenFloor.GetComponent<NetworkObject>().Spawn();

            // set the floor type
            no_SunkenFloor.GetComponent<SunkenFloor>().SetTypeAndSprite(sunkenFloorSpawner.SunkenFloorType);

            // get list of button spawners
            var buttonSpawners = new List<SunkenFloorButtonSpawner>(GetComponentsInChildren<SunkenFloorButtonSpawner>());

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
            no_SunkenFloor.GetComponent<SunkenFloor>().NumberButtons = buttonSpawners.Count;

            // spawn children buttons
            for (int j = 0; j < buttonSpawners.Count; j++)
            {
                var buttonSpawner = buttonSpawners[j];

                // insta/spawn network buttno
                var no_Button = Instantiate(sunkenFloorSpawner.SunkenFloorButtonPrefab);
                no_Button.transform.position = buttonSpawner.transform.position;
                no_Button.GetComponent<NetworkObject>().Spawn();

                // set network object buttons parent to the sunken floor
                no_Button.transform.parent = no_SunkenFloor.transform;
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

            // destroy the sunkenFloorSpawner
            Destroy(sunkenFloorSpawners[i].gameObject);
            sunkenFloorSpawners.RemoveAt(i);
        }

        // destroy list
        sunkenFloorSpawners.Clear();
    }
}


