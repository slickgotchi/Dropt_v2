using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Level
{
    // Ape Doors
    public partial class NetworkLevel : NetworkBehaviour
    {
        public void CreateSpawners_SunkenFloorsAndButtons()
        {
            var spawners = new List<SunkenFloorButtonGroupSpawner>(GetComponentsInChildren<SunkenFloorButtonGroupSpawner>());

            for (int i = 0; i < spawners.Count; i++)
            {
                var no_buttonGroup = Object.Instantiate(spawners[i].SunkenFloorButtonGroupPrefab);

                AddLevelSpawnComponent(no_buttonGroup, spawners[i].spawnerId, spawners[i].GetComponent<Spawner_SpawnCondition>());

                CreateSunkenFloorButtons(spawners[i], no_buttonGroup);
                CreateSunkenFloors(spawners[i], no_buttonGroup);

            }
        }

        private void CreateSunkenFloorButtons(SunkenFloorButtonGroupSpawner buttonGroupSpawner, GameObject no_buttonGroup)
        {
            // get button group down to the required size
            var buttonGroup = no_buttonGroup.GetComponent<SunkenFloorButtonGroup>();
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
                var no_button = Object.Instantiate(buttonGroupSpawner.SunkenFloorButtonPrefab);
                no_button.transform.position = buttonSpawner.transform.position;
                no_button.GetComponent<SunkenFloorButton>().Type.Value = buttonGroupSpawner.SunkenFloorType;
                no_button.GetComponent<SunkenFloorButton>().spawnerId = buttonGroupSpawner.spawnerId;

                AddLevelSpawnComponent(no_button, buttonGroupSpawner.spawnerId, buttonGroupSpawner.GetComponent<Spawner_SpawnCondition>());
            }
        }

        private void CreateSunkenFloors(SunkenFloorButtonGroupSpawner buttonGroupSpawner, GameObject no_buttonGroup)
        {
            // iterate over all apedoor spawners
            foreach (var floorSpawner in buttonGroupSpawner.SunkenFloorSpawners)
            {
                var no_sunkenFloor = Object.Instantiate(floorSpawner.SunkenFloorPrefab);
                no_sunkenFloor.transform.position = floorSpawner.transform.position;
                no_sunkenFloor.GetComponent<SunkenFloor>().SetTypeAndState(buttonGroupSpawner.SunkenFloorType, SunkenFloorState.Lowered);
                no_sunkenFloor.GetComponent<SunkenFloor>().spawnerId = buttonGroupSpawner.spawnerId;
                no_buttonGroup.GetComponent<SunkenFloorButtonGroup>().SunkenFloors.Add(no_sunkenFloor);

                AddLevelSpawnComponent(no_sunkenFloor, buttonGroupSpawner.spawnerId, floorSpawner.GetComponent<Spawner_SpawnCondition>());
            }
        }
    }
}
