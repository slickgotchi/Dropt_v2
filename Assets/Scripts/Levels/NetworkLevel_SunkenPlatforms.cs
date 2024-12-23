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
            List<SunkenFloorButtonGroupSpawner> spawners = new List<SunkenFloorButtonGroupSpawner>(GetComponentsInChildren<SunkenFloorButtonGroupSpawner>());
            for (int i = 0; i < spawners.Count; i++)
            {
                CreateSunkenFloorButtons(spawners[i]);
                CreateSunkenFloors(spawners[i]);
            }
        }

        public void CreateSpawners_CrystalPlatformAndButtons()
        {
            List<CrystalPlatformButtonGroupSpawner> spawners = new List<CrystalPlatformButtonGroupSpawner>(GetComponentsInChildren<CrystalPlatformButtonGroupSpawner>());
            for (int i = 0; i < spawners.Count; i++)
            {
                CreateCrystalPlatformButtons(spawners[i]);
                CreateCrystalPlatforms(spawners[i]);
            }
        }

        private void CreateSunkenFloorButtons(SunkenFloorButtonGroupSpawner buttonGroupSpawner)
        {
            // get button group down to the required size
            for (int j = 0; j < buttonGroupSpawner.transform.childCount; j++)
            {
                if (buttonGroupSpawner.transform.childCount > buttonGroupSpawner.NumberButtons)
                {
                    int randIndex = Random.Range(0, buttonGroupSpawner.transform.childCount);
                    Transform button = buttonGroupSpawner.transform.GetChild(randIndex);
                    button.parent = null;
                    Destroy(button.gameObject);
                }
                else
                {
                    break;
                }
            }

            // create buttons
            for (int j = 0; j < buttonGroupSpawner.transform.childCount; j++)
            {
                Transform buttonSpawner = buttonGroupSpawner.transform.GetChild(j);
                GameObject no_button = Instantiate(buttonGroupSpawner.SunkenFloorButtonPrefab);
                no_button.transform.position = buttonSpawner.transform.position;
                SunkenFloorButton sunkenFloorButton = no_button.GetComponent<SunkenFloorButton>();
                sunkenFloorButton.initType = buttonGroupSpawner.SunkenFloorType;
                sunkenFloorButton.spawnerId = buttonGroupSpawner.spawnerId;

                LevelSpawnManager.Instance.AddLevelSpawnComponent(no_button,
                                       buttonGroupSpawner.spawnerId,
                                       null,
                                       buttonGroupSpawner.GetComponent<Spawner_SpawnCondition>());
            }
        }

        private void CreateCrystalPlatformButtons(CrystalPlatformButtonGroupSpawner buttonGroupSpawner)
        {
            // get button group down to the required size
            for (int j = 0; j < buttonGroupSpawner.transform.childCount; j++)
            {
                if (buttonGroupSpawner.transform.childCount > buttonGroupSpawner.NumberButtons)
                {
                    int randIndex = Random.Range(0, buttonGroupSpawner.transform.childCount);
                    Transform button = buttonGroupSpawner.transform.GetChild(randIndex);
                    button.parent = null;
                    Destroy(button.gameObject);
                }
                else
                {
                    break;
                }
            }

            // create buttons
            for (int j = 0; j < buttonGroupSpawner.transform.childCount; j++)
            {
                Transform buttonSpawner = buttonGroupSpawner.transform.GetChild(j);
                GameObject no_button = Instantiate(buttonGroupSpawner.CrystalPlatformButtonPrefab);
                no_button.transform.position = buttonSpawner.transform.position;
                CrystalPlatformButton crystalPlatformButton = no_button.GetComponent<CrystalPlatformButton>();
                crystalPlatformButton.initType = buttonGroupSpawner.CrystalPlatformType;
                crystalPlatformButton.spawnerId = buttonGroupSpawner.spawnerId;

                LevelSpawnManager.Instance.AddLevelSpawnComponent(no_button,
                                       buttonGroupSpawner.spawnerId,
                                       null,
                                       buttonGroupSpawner.GetComponent<Spawner_SpawnCondition>());
            }
        }

        private void CreateSunkenFloors(SunkenFloorButtonGroupSpawner buttonGroupSpawner)
        {
            // iterate over all apedoor spawners
            foreach (SunkenFloorSpawner floorSpawner in buttonGroupSpawner.SunkenFloorSpawners)
            {
                GameObject no_sunkenFloor = Instantiate(floorSpawner.SunkenFloorPrefab);
                no_sunkenFloor.transform.position = floorSpawner.transform.position;
                SunkenFloor sunkenFloor = no_sunkenFloor.GetComponent<SunkenFloor>();
                sunkenFloor.initType = buttonGroupSpawner.SunkenFloorType;
                sunkenFloor.initState = PlatformState.Lowered;
                sunkenFloor.spawnerId = buttonGroupSpawner.spawnerId;

                LevelSpawnManager.Instance.AddLevelSpawnComponent(no_sunkenFloor,
                                       buttonGroupSpawner.spawnerId,
                                       null,
                                       floorSpawner.GetComponent<Spawner_SpawnCondition>());
            }
        }

        private void CreateCrystalPlatforms(CrystalPlatformButtonGroupSpawner buttonGroupSpawner)
        {
            // iterate over all apedoor spawners
            foreach (CrystalPlatformSpawner floorSpawner in buttonGroupSpawner.CrystalPlatformSpawners)
            {
                GameObject no_sunkenFloor = Instantiate(floorSpawner.CrystalPlatformPrefab);
                no_sunkenFloor.transform.position = floorSpawner.transform.position;
                CrystalPlatform crystalPlatform = no_sunkenFloor.GetComponent<CrystalPlatform>();
                crystalPlatform.initType = buttonGroupSpawner.CrystalPlatformType;
                crystalPlatform.initState = PlatformState.Lowered;
                crystalPlatform.spawnerId = buttonGroupSpawner.spawnerId;

                LevelSpawnManager.Instance.AddLevelSpawnComponent(no_sunkenFloor,
                                       buttonGroupSpawner.spawnerId,
                                       null,
                                       floorSpawner.GetComponent<Spawner_SpawnCondition>());
            }
        }
    }
}
