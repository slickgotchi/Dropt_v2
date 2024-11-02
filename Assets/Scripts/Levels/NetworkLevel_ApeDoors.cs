using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Level
{
    // Ape Doors
    public partial class NetworkLevel : NetworkBehaviour
    {
        public void CreateSpawners_ApeDoorsAndButtons()
        {
            var buttonGroupSpawners = new List<ApeDoorButtonGroupSpawner>(GetComponentsInChildren<ApeDoorButtonGroupSpawner>());
            for (int i = 0; i < buttonGroupSpawners.Count; i++)
            {
                CreateApeDoorButtons(buttonGroupSpawners[i]);
                CreateApeDoors(buttonGroupSpawners[i]);
            }
        }

        private void CreateApeDoorButtons(ApeDoorButtonGroupSpawner buttonGroupSpawner)
        {
            // get button group down to the required size
            for (int j = 0; j < buttonGroupSpawner.transform.childCount; j++)
            {
                if (buttonGroupSpawner.transform.childCount > buttonGroupSpawner.NumberButtons)
                {
                    var randIndex = UnityEngine.Random.Range(0, buttonGroupSpawner.transform.childCount);
                    var button = buttonGroupSpawner.transform.GetChild(randIndex);
                    button.parent = null;
                    Object.Destroy(button.gameObject);
                }
                else
                {
                    break;
                }
            }

            // create buttons
            for (int j = 0; j < buttonGroupSpawner.transform.childCount; j++)
            {
                var buttonSpawner = buttonGroupSpawner.transform.GetChild(j);
                var no_button = Object.Instantiate(buttonGroupSpawner.ApeDoorButtonPrefab);
                no_button.transform.position = buttonSpawner.transform.position;
                no_button.GetComponent<ApeDoorButton>().initType = buttonGroupSpawner.ApeDoorType;
                no_button.GetComponent<ApeDoorButton>().spawnerId = buttonGroupSpawner.spawnerId;

                AddLevelSpawnComponent(no_button, buttonGroupSpawner.spawnerId, buttonGroupSpawner.GetComponent<Spawner_SpawnCondition>());
            }
        }

        private void CreateApeDoors(ApeDoorButtonGroupSpawner buttonGroupSpawner)
        {
            // iterate over all apedoor spawners
            foreach (var apeDoorSpawner in buttonGroupSpawner.ApeDoorSpawners)
            {
                var no_apeDoor = Object.Instantiate(apeDoorSpawner.ApeDoorPrefab);
                no_apeDoor.transform.position = apeDoorSpawner.transform.position;
                no_apeDoor.GetComponent<ApeDoor>().initType = buttonGroupSpawner.ApeDoorType;
                no_apeDoor.GetComponent<ApeDoor>().initState = DoorState.Closed;
                no_apeDoor.GetComponent<ApeDoor>().spawnerId = buttonGroupSpawner.spawnerId;

                AddLevelSpawnComponent(no_apeDoor, buttonGroupSpawner.spawnerId, apeDoorSpawner.GetComponent<Spawner_SpawnCondition>());
            }
        }
    }
}
