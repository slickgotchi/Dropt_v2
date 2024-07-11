using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Level
{
    public static class ApeDoorFactory
    {
        public static void CreateApeDoors(GameObject parent)
        {
            var buttonGroupSpawners = new List<ApeDoorButtonGroupSpawner>(parent.GetComponentsInChildren<ApeDoorButtonGroupSpawner>());

            foreach (var buttonGroupSpawner in buttonGroupSpawners)
            {
                var no_buttonGroup = Object.Instantiate(buttonGroupSpawner.ApeDoorButtonGroupPrefab, buttonGroupSpawner.transform);
                no_buttonGroup.GetComponent<NetworkObject>().Spawn();
                no_buttonGroup.GetComponent<NetworkObject>().TrySetParent(parent);

                CreateButtons(buttonGroupSpawner, no_buttonGroup);
                CreateApeDoors(buttonGroupSpawner, no_buttonGroup);

                CleanupFactory.DestroyAllChildren(buttonGroupSpawner.transform);
            }
        }

        private static void CreateButtons(ApeDoorButtonGroupSpawner buttonGroupSpawner, GameObject no_buttonGroup)
        {
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

            for (int j = 0; j < buttonGroupSpawner.transform.childCount; j++)
            {
                var buttonSpawner = buttonGroupSpawner.transform.GetChild(j);
                var no_button = Object.Instantiate(buttonGroupSpawner.ApeDoorButtonPrefab);
                no_button.transform.position = buttonSpawner.transform.position;
                no_button.GetComponent<NetworkObject>().Spawn();
                no_button.GetComponent<ApeDoorButton>().Type.Value = buttonGroupSpawner.ApeDoorType;
                no_button.GetComponent<NetworkObject>().TrySetParent(no_buttonGroup);
            }
        }

        private static void CreateApeDoors(ApeDoorButtonGroupSpawner buttonGroupSpawner, GameObject no_buttonGroup)
        {
            foreach (var apeDoorSpawner in buttonGroupSpawner.ApeDoorSpawners)
            {
                var no_apeDoor = Object.Instantiate(apeDoorSpawner.ApeDoorPrefab, apeDoorSpawner.transform);
                no_apeDoor.GetComponent<NetworkObject>().Spawn();
                no_apeDoor.GetComponent<ApeDoor>().SetTypeAndState(buttonGroupSpawner.ApeDoorType, DoorState.Closed);
                no_buttonGroup.GetComponent<ApeDoorButtonGroup>().ApeDoors.Add(no_apeDoor);
            }
        }
    }
}
