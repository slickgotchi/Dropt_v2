using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkLevel : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // search for sunken floors spawners and button spawners
        var sunkenFloorSpawners = new List<SunkenFloor3x3Spawner>(GetComponentsInChildren<SunkenFloor3x3Spawner>());
        Debug.Log(sunkenFloorSpawners.Count);
        for (int i = 0; i < sunkenFloorSpawners.Count; i++)
        {
            var sfs = sunkenFloorSpawners[i];
            Debug.Log(sfs);

            // insta/spawn network sunken floor
            var nwSunkenFloor = Instantiate(sfs.SunkenFloor3x3Prefab);
            nwSunkenFloor.transform.position = sfs.transform.position;
            nwSunkenFloor.GetComponent<NetworkObject>().Spawn();

            // set the floor type
            nwSunkenFloor.GetComponent<SunkenFloor>().SetTypeAndSprite(sfs.SunkenFloorType);

            // spawn children buttons
            var sfsButtons = new List<SunkenFloorButtonSpawner>(GetComponentsInChildren<SunkenFloorButtonSpawner>());
            for (int j = 0; j < sfsButtons.Count; j++)
            {
                var button = sfsButtons[j];

                // insta/spawn network buttno
                var nwButton = Instantiate(sfs.SunkenFloorButtonPrefab);
                nwButton.transform.position = button.transform.position;
                nwButton.GetComponent<NetworkObject>().Spawn();
            }

            // destroy button list
            sfsButtons.Clear();
        }

        // destroy list
        sunkenFloorSpawners.Clear();
    }
}
