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
            var spawners = new List<ApeDoorButtonGroupSpawner>(GetComponentsInChildren<ApeDoorButtonGroupSpawner>());

            for (int i = 0; i < spawners.Count; i++)
            {
                var no_buttonGroup = Object.Instantiate(spawners[i].ApeDoorButtonGroupPrefab);

                AddLevelSpawnComponent(no_buttonGroup, spawners[i].spawnerId, spawners[i].GetComponent<Spawner_SpawnCondition>());

                CreateApeDoorButtons(spawners[i], no_buttonGroup);
                CreateApeDoors(spawners[i], no_buttonGroup);

            }
        }

        private void CreateApeDoorButtons(ApeDoorButtonGroupSpawner buttonGroupSpawner, GameObject no_buttonGroup)
        {
            // get button group down to the required size
            var buttonGroup = no_buttonGroup.GetComponent<ApeDoorButtonGroup>();
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

            buttonGroup.NumberButtons = buttonGroupSpawner.transform.childCount;

            // create buttons
            for (int j = 0; j < buttonGroupSpawner.transform.childCount; j++)
            {
                var buttonSpawner = buttonGroupSpawner.transform.GetChild(j);
                var no_button = Object.Instantiate(buttonGroupSpawner.ApeDoorButtonPrefab);
                no_button.transform.position = buttonSpawner.transform.position;
                no_button.GetComponent<ApeDoorButton>().Type.Value = buttonGroupSpawner.ApeDoorType;
                no_button.GetComponent<ApeDoorButton>().spawnerId = buttonGroupSpawner.spawnerId;

                AddLevelSpawnComponent(no_button, buttonGroupSpawner.spawnerId, buttonGroupSpawner.GetComponent<Spawner_SpawnCondition>());

                //no_button.GetComponent<NetworkObject>().Spawn();
                //no_button.GetComponent<NetworkObject>().TrySetParent(no_buttonGroup);
            }
        }

        private void CreateApeDoors(ApeDoorButtonGroupSpawner buttonGroupSpawner, GameObject no_buttonGroup)
        {
            // iterate over all apedoor spawners
            foreach (var apeDoorSpawner in buttonGroupSpawner.ApeDoorSpawners)
            {
                var no_apeDoor = Object.Instantiate(apeDoorSpawner.ApeDoorPrefab);
                no_apeDoor.transform.position = apeDoorSpawner.transform.position;
                no_apeDoor.GetComponent<ApeDoor>().SetTypeAndState(buttonGroupSpawner.ApeDoorType, DoorState.Closed);
                no_apeDoor.GetComponent<ApeDoor>().spawnerId = buttonGroupSpawner.spawnerId;
                no_buttonGroup.GetComponent<ApeDoorButtonGroup>().ApeDoors.Add(no_apeDoor);

                AddLevelSpawnComponent(no_apeDoor, buttonGroupSpawner.spawnerId, apeDoorSpawner.GetComponent<Spawner_SpawnCondition>());
            }
        }
    }
}
