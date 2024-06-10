using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class NetworkLevel : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        CreateSunkenFloors();
        CreateApeDoors();
        CreateNetworkObjectSpawners();
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

    private struct Spawn
    {
        public string Name;
        public float Chance;
    }

    public void CreateNetworkObjectSpawners()
    {
        var no_spawners = new List<NetworkObjectSpawner>(GetComponentsInChildren<NetworkObjectSpawner>());
        for (int i = 0; i < no_spawners.Count; i++)
        {
            var no_spawner = no_spawners[i];
            NormalizeNetworkObjectSpawner(ref no_spawner);

            if (no_spawner.transform.childCount <= 0)
            {
                Debug.Log("No spawn points attached as children to NetworkObjectSpawner");
                continue;
            }

            // create one list of the spawn names and spawn chances
            List<Spawn> spawns = new List<Spawn>();
            for (int j = 0; j < no_spawner.SpawnEnemies.Length; j++)
            {
                spawns.Add(new Spawn
                {
                    Name = no_spawner.SpawnEnemies[j].EnemyPrefabType.ToString(),
                    Chance = no_spawner.SpawnEnemies[j].SpawnChance,
                });
            }

            for (int j = 0; j < no_spawner.SpawnDestructibles.Length; j++)
            {
                spawns.Add(new Spawn
                {
                    Name = no_spawner.SpawnDestructibles[j].DestructiblePrefabType.ToString(),
                    Chance = no_spawner.SpawnDestructibles[j].SpawnChance,
                });
            }

            for (int j = 0; j < no_spawner.SpawnInteractables.Length; j++)
            {
                spawns.Add(new Spawn
                {
                    Name = no_spawner.SpawnInteractables[j].InteractablePrefabType.ToString(),
                    Chance = no_spawner.SpawnInteractables[j].SpawnChance,
                });
            }

            for (int j = 0; j < no_spawner.SpawnProps.Length; j++)
            {
                spawns.Add(new Spawn
                {
                    Name = no_spawner.SpawnProps[j].PropPrefabType.ToString(),
                    Chance = no_spawner.SpawnProps[j].SpawnChance,
                });
            }

            // iterate through the spawners spawn points
            for (int j = 0; j < no_spawner.transform.childCount; j++)
            {
                var spawnPoint = no_spawner.transform.GetChild(j);
                var randValue = UnityEngine.Random.Range(0f, 1f);

                for (int k = 0; k < spawns.Count; k++)
                {
                    var spawn = spawns[k];

                    if (randValue < spawn.Chance)
                    {
                        var spawnObject = Prefabs_NetworkObject.Instance.GetNetworkObjectByName(spawn.Name);
                        if (spawnObject != null)
                        {
                            var no_object = Instantiate(spawnObject, spawnPoint);
                            no_object.gameObject.GetComponent<NetworkObject>().Spawn();
                        } else
                        {
                            Debug.Log(spawn.Name + " does not exist in Prefabs_NetworkObject");
                        }
                        break;
                    } else
                    {
                        randValue -= spawn.Chance;
                    }
                }
            }
        }

        // destroy all spawners
        while (no_spawners.Count > 0)
        {
            Destroy(no_spawners[0].gameObject);
            no_spawners.RemoveAt(0);
        }
        no_spawners.Clear();
    }

    //private GameObject GetEnemyPrefab(EnemySpawnType type)
    //{
    //    switch (type)
    //    {
    //        case EnemySpawnType.BombSnail: return Prefabs_NetworkObject_Enemy.Instance.BombSnail;
    //        case EnemySpawnType.FudSpirit: return Prefabs_NetworkObject_Enemy.Instance.FudSpirit;
    //        case EnemySpawnType.FudWisp: return Prefabs_NetworkObject_Enemy.Instance.FudWisp;
    //        case EnemySpawnType.FussPot: return Prefabs_NetworkObject_Enemy.Instance.FussPot;
    //        case EnemySpawnType.GasBag: return Prefabs_NetworkObject_Enemy.Instance.GasBag;
    //        case EnemySpawnType.GeodeShade: return Prefabs_NetworkObject_Enemy.Instance.GeodeShade;
    //        case EnemySpawnType.LeafShade: return Prefabs_NetworkObject_Enemy.Instance.LeafShade;
    //        case EnemySpawnType.SentryBot: return Prefabs_NetworkObject_Enemy.Instance.SentryBot;
    //        case EnemySpawnType.Snail: return Prefabs_NetworkObject_Enemy.Instance.Snail;
    //        case EnemySpawnType.Spider: return Prefabs_NetworkObject_Enemy.Instance.Spider;
    //        case EnemySpawnType.SpiderPod: return Prefabs_NetworkObject_Enemy.Instance.SpiderPod;
    //        case EnemySpawnType.WispHollow: return Prefabs_NetworkObject_Enemy.Instance.WispHollow;
    //        default : return null;
    //    }
    //}

    //private GameObject GetDestructiblePrefab(DestructibleSpawnType type)
    //{
    //    switch (type)
    //    {
    //        case DestructibleSpawnType.BlueCrate: return Prefabs_NetworkObject_Destructible.Instance.BlueCrate;
    //        case DestructibleSpawnType.BlueFern: return Prefabs_NetworkObject_Destructible.Instance.BlueFern;
    //        case DestructibleSpawnType.Geode_Plain: return Prefabs_NetworkObject_Destructible.Instance.Geode_Plain;
    //        case DestructibleSpawnType.Geode_Common: return Prefabs_NetworkObject_Destructible.Instance.Geode_Common;
    //        case DestructibleSpawnType.Geode_Uncommon: return Prefabs_NetworkObject_Destructible.Instance.Geode_Uncommon;
    //        case DestructibleSpawnType.Geode_Rare: return Prefabs_NetworkObject_Destructible.Instance.Geode_Rare;
    //        case DestructibleSpawnType.Geode_Legendary: return Prefabs_NetworkObject_Destructible.Instance.Geode_Legendary;
    //        case DestructibleSpawnType.Geode_Mythical: return Prefabs_NetworkObject_Destructible.Instance.Geode_Mythical;
    //        case DestructibleSpawnType.Geode_Godlike: return Prefabs_NetworkObject_Destructible.Instance.Geode_Godlike;
    //        default: return null;
    //    }
    //}

    //private GameObject GetInteractablePrefab(InteractableSpawnType type)
    //{
    //    switch (type)
    //    {
    //        case InteractableSpawnType.Hole: return Prefabs_NetworkObject_Interactable.Instance.Hole;
    //        case InteractableSpawnType.GotchiSelectPortal: return Prefabs_NetworkObject_Interactable.Instance.GotchiSelectPortal;
    //        case InteractableSpawnType.EscapePortal: return Prefabs_NetworkObject_Interactable.Instance.EscapePortal;
    //        default: return null;
    //    }
    //}

    //private GameObject GetPropPrefabEntity(PropSpawnType type)
    //{
    //    switch (type)
    //    {
    //        case PropSpawnType.LightPole: return Prefabs_NetworkObject_Prop.Instance.LightPole;
    //        case PropSpawnType.RuinBlock: return Prefabs_NetworkObject_Prop.Instance.RuinBlock;
    //        default: return null;
    //    }
    //}

    private void NormalizeNetworkObjectSpawner(ref NetworkObjectSpawner no_spawner)
    {
        var sum = no_spawner.NoSpawnChance;

        foreach (var spawnPrefab in no_spawner.SpawnEnemies)
        {
            sum += spawnPrefab.SpawnChance;
        }

        foreach (var spawnPrefab in no_spawner.SpawnDestructibles)
        {
            sum += spawnPrefab.SpawnChance;
        }

        foreach (var spawnPrefab in no_spawner.SpawnInteractables)
        {
            sum += spawnPrefab.SpawnChance;
        }

        foreach (var spawnPrefab in no_spawner.SpawnProps)
        {
            sum += spawnPrefab.SpawnChance;
        }

        if (sum <= 0) return;

        no_spawner.NoSpawnChance /= sum;

        foreach (var spawnPrefab in no_spawner.SpawnEnemies)
        {
            spawnPrefab.SpawnChance /= sum;
        }

        foreach (var spawnPrefab in no_spawner.SpawnDestructibles)
        {
            spawnPrefab.SpawnChance /= sum;
        }

        foreach (var spawnPrefab in no_spawner.SpawnInteractables)
        {
            spawnPrefab.SpawnChance /= sum;
        }

        foreach (var spawnPrefab in no_spawner.SpawnProps)
        {
            spawnPrefab.SpawnChance /= sum;
        }

    }
}


