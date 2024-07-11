using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Level
{
    public static class SunkenFloorFactory
    {
        public static void CreateSunkenFloors(GameObject parent)
        {
            var buttonGroupSpawners = new List<SunkenFloorButtonGroupSpawner>(parent.GetComponentsInChildren<SunkenFloorButtonGroupSpawner>());

            foreach (var buttonGroupSpawner in buttonGroupSpawners)
            {
                var no_buttonGroup = Object.Instantiate(buttonGroupSpawner.SunkenFloorButtonGroupPrefab, buttonGroupSpawner.transform);
                no_buttonGroup.GetComponent<NetworkObject>().Spawn();
                no_buttonGroup.GetComponent<NetworkObject>().TrySetParent(parent);

                CreateButtons(buttonGroupSpawner, no_buttonGroup);
                CreateSunkenFloors(buttonGroupSpawner, no_buttonGroup);

                CleanupFactory.DestroyAllChildren(buttonGroupSpawner.transform);
            }
        }

        private static void CreateButtons(SunkenFloorButtonGroupSpawner buttonGroupSpawner, GameObject no_buttonGroup)
        {
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

            for (int j = 0; j < buttonGroupSpawner.transform.childCount; j++)
            {
                var buttonSpawner = buttonGroupSpawner.transform.GetChild(j);
                var no_button = Object.Instantiate(buttonGroupSpawner.SunkenFloorButtonPrefab);
                no_button.transform.position = buttonSpawner.transform.position;
                no_button.GetComponent<NetworkObject>().Spawn();
                no_button.GetComponent<SunkenFloorButton>().Type.Value = buttonGroupSpawner.SunkenFloorType;
                no_button.GetComponent<NetworkObject>().TrySetParent(no_buttonGroup);
            }
        }

        private static void CreateSunkenFloors(SunkenFloorButtonGroupSpawner buttonGroupSpawner, GameObject no_buttonGroup)
        {
            foreach (var sunkenFloorSpawner in buttonGroupSpawner.SunkenFloorSpawners)
            {
                var no_sunkenFloor = Object.Instantiate(sunkenFloorSpawner.SunkenFloorPrefab, sunkenFloorSpawner.transform);
                no_sunkenFloor.GetComponent<NetworkObject>().Spawn();
                no_sunkenFloor.GetComponent<SunkenFloor>().SetTypeAndState(buttonGroupSpawner.SunkenFloorType, SunkenFloorState.Lowered);
                no_buttonGroup.GetComponent<SunkenFloorButtonGroup>().SunkenFloors.Add(no_sunkenFloor);
            }
        }
    }
}
