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
            List<ApeDoorButtonGroupSpawner> buttonGroupSpawners = new List<ApeDoorButtonGroupSpawner>(GetComponentsInChildren<ApeDoorButtonGroupSpawner>());
            for (int i = 0; i < buttonGroupSpawners.Count; i++)
            {
                CreateApeDoorButtons(buttonGroupSpawners[i]);
                CreateApeDoors(buttonGroupSpawners[i]);
            }

#if UNITY_EDITOR
            List<ApeDoorType> doors = new List<ApeDoorType>();
            foreach (var buttonGroupSpawner in buttonGroupSpawners)
            {
                if (doors.Contains(buttonGroupSpawner.ApeDoorType))
                {
                    Debug.LogWarning($"Ape Door Of Type {buttonGroupSpawner.ApeDoorType} is assign to multiple ape door spawner");
                }
                doors.Add(buttonGroupSpawner.ApeDoorType);
            }
#endif
        }

        public void CreateSpawners_CrystalDoorsAndButtons()
        {
            List<CrystalDoorButtonGroupSpawner> buttonGroupSpawners = new List<CrystalDoorButtonGroupSpawner>(GetComponentsInChildren<CrystalDoorButtonGroupSpawner>());
            for (int i = 0; i < buttonGroupSpawners.Count; i++)
            {
                CreateCrystalDoorButtons(buttonGroupSpawners[i]);
                CreateCrystalDoors(buttonGroupSpawners[i]);
            }
#if UNITY_EDITOR
            List<CrystalDoorType> doors = new List<CrystalDoorType>();
            foreach (var buttonGroupSpawner in buttonGroupSpawners)
            {
                if (doors.Contains(buttonGroupSpawner.CrystalDoorType))
                {
                    Debug.LogWarning($"Crystal Door Of Type {buttonGroupSpawner.CrystalDoorType} is assign to multiple crystal door spawner");
                }
                doors.Add(buttonGroupSpawner.CrystalDoorType);
            }
#endif
        }

        private void CreateApeDoorButtons(ApeDoorButtonGroupSpawner buttonGroupSpawner)
        {
            // get button group down to the required size
            for (int j = 0; j < buttonGroupSpawner.transform.childCount; j++)
            {
                if (buttonGroupSpawner.transform.childCount <= buttonGroupSpawner.NumberButtons)
                {
                    break;
                }

                int randIndex = Random.Range(0, buttonGroupSpawner.transform.childCount);
                Transform button = buttonGroupSpawner.transform.GetChild(randIndex);
                button.parent = null;
                Destroy(button.gameObject);
            }

            // create buttons
            for (int j = 0; j < buttonGroupSpawner.transform.childCount; j++)
            {
                Transform buttonSpawner = buttonGroupSpawner.transform.GetChild(j);
                GameObject no_button = Instantiate(buttonGroupSpawner.ApeDoorButtonPrefab);
                no_button.transform.position = buttonSpawner.transform.position;
                ApeDoorButton apeDoorButton = no_button.GetComponent<ApeDoorButton>();
                apeDoorButton.initType = buttonGroupSpawner.ApeDoorType;
                apeDoorButton.spawnerId = buttonGroupSpawner.spawnerId;

                LevelSpawnManager.Instance.AddLevelSpawnComponent(no_button,
                           buttonGroupSpawner.spawnerId,
                           null,
                           buttonGroupSpawner.GetComponent<Spawner_SpawnCondition>());
            }
        }

        private void CreateCrystalDoorButtons(CrystalDoorButtonGroupSpawner buttonGroupSpawner)
        {
            // get button group down to the required size
            for (int j = 0; j < buttonGroupSpawner.transform.childCount; j++)
            {
                if (buttonGroupSpawner.transform.childCount <= buttonGroupSpawner.NumberButtons)
                {
                    break;
                }

                int randIndex = Random.Range(0, buttonGroupSpawner.transform.childCount);
                Transform button = buttonGroupSpawner.transform.GetChild(randIndex);
                button.parent = null;
                Destroy(button.gameObject);
            }

            for (int j = 0; j < buttonGroupSpawner.transform.childCount; j++)
            {
                Transform buttonSpawner = buttonGroupSpawner.transform.GetChild(j);
                GameObject no_button = Instantiate(buttonGroupSpawner.CrystalDoorButtonPrefab);
                no_button.transform.position = buttonSpawner.transform.position;
                CrystalDoorButton crystalDoorButton = no_button.GetComponent<CrystalDoorButton>();
                crystalDoorButton.initType = buttonGroupSpawner.CrystalDoorType;
                crystalDoorButton.spawnerId = buttonGroupSpawner.spawnerId;
                LevelSpawnManager.Instance.AddLevelSpawnComponent(no_button,
                                       buttonGroupSpawner.spawnerId,
                                       null,
                                       buttonGroupSpawner.GetComponent<Spawner_SpawnCondition>());
            }
        }

        private void CreateApeDoors(ApeDoorButtonGroupSpawner buttonGroupSpawner)
        {
            // iterate over all apedoor spawners
            foreach (var apeDoorSpawner in buttonGroupSpawner.ApeDoorSpawners)
            {
                GameObject no_apeDoor = Instantiate(apeDoorSpawner.ApeDoorPrefab);
                no_apeDoor.transform.position = apeDoorSpawner.transform.position;
                ApeDoor apeDoor = no_apeDoor.GetComponent<ApeDoor>();
                apeDoor.initType = buttonGroupSpawner.ApeDoorType;
                apeDoor.initState = DoorState.Closed;
                apeDoor.spawnerId = buttonGroupSpawner.spawnerId;

                LevelSpawnManager.Instance.AddLevelSpawnComponent(no_apeDoor,
                                       buttonGroupSpawner.spawnerId,
                                       null,
                                       apeDoorSpawner.GetComponent<Spawner_SpawnCondition>());
            }
        }

        private void CreateCrystalDoors(CrystalDoorButtonGroupSpawner buttonGroupSpawner)
        {
            // iterate over all apedoor spawners
            foreach (var crystalDoorSpawner in buttonGroupSpawner.CrystalDoorSpawners)
            {
                GameObject no_crystalDoor = Instantiate(crystalDoorSpawner.GetRandomDoorPrefab());
                no_crystalDoor.transform.position = crystalDoorSpawner.transform.position;
                CrystalDoor crystalDoor = no_crystalDoor.GetComponent<CrystalDoor>();
                crystalDoor.initType = buttonGroupSpawner.CrystalDoorType;
                crystalDoor.initState = DoorState.Closed;
                crystalDoor.spawnerId = buttonGroupSpawner.spawnerId;

                LevelSpawnManager.Instance.AddLevelSpawnComponent(no_crystalDoor,
                                       buttonGroupSpawner.spawnerId,
                                       null,
                                       crystalDoorSpawner.GetComponent<Spawner_SpawnCondition>());
            }

        }
    }
}