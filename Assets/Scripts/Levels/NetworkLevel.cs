using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class NetworkLevel : NetworkBehaviour
{
    private struct NetworkObjectSpawn
    {
        public string Name;
        public float Chance;
    }

    private List<Vector3> m_availablePlayerSpawnPoints = new List<Vector3>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // if on first level layer, ensure spawn points list is clear
            if (LevelManager.Instance.LevelSpawningCount == 1)
            {
                m_availablePlayerSpawnPoints.Clear();
            }

            CreateSunkenFloors();
            CreateApeDoors();
            CreateNetworkObjectSpawners();
            //CreatePlayerSpawnPoints();
            CreateSubLevels();

            // level spawned, decrease spawning count
            LevelManager.Instance.LevelSpawningCount--;
        }

        // if we're the client, we need to clean up the spawner objects not required client side
        if (IsClient)
        {
            var no_sunkenFloorSpawners = new List<SunkenFloor3x3Spawner>(GetComponentsInChildren<SunkenFloor3x3Spawner>());
            for (int i = 0; i < no_sunkenFloorSpawners.Count; i++)
            {
                Destroy(no_sunkenFloorSpawners[i].gameObject);
            }

            var no_apeDoorSpawners = new List<ApeDoorSpawner>(GetComponentsInChildren<ApeDoorSpawner>());
            for (int i = 0; i < no_apeDoorSpawners.Count; i++)
            {
                Destroy(no_apeDoorSpawners[i].gameObject);
            }

            var no_spawners = new List<NetworkObjectSpawner>(GetComponentsInChildren<NetworkObjectSpawner>());
            for (int i = 0; i < no_spawners.Count; i++)
            {
                Destroy(no_spawners[i].gameObject);
            }

            var no_playerSpawnPointsList = new List<PlayerSpawnPoints>(GetComponentsInChildren<PlayerSpawnPoints>());
            for (int i = 0; i < no_playerSpawnPointsList.Count; i++)
            {
                Destroy(no_playerSpawnPointsList[i].gameObject);
            }
        }

    }

    public override void OnNetworkDespawn()
    {

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
            no_sunkenFloor.GetComponent<SunkenFloor>().SetTypeAndState(sunkenFloorSpawner.SunkenFloorType, SunkenFloorState.Lowered);

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
                no_Button.GetComponent<SunkenFloorButton>().SetTypeAndState(sunkenFloorSpawner.SunkenFloorType, ButtonState.Up);

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
            no_apeDoor.GetComponent<ApeDoor>().SetTypeAndState(apeDoorSpawner.ApeDoorType, DoorState.Closed);

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
                no_Button.GetComponent<ApeDoorButton>().SetTypeAndState(apeDoorSpawner.ApeDoorType, ButtonState.Up);

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
            List<NetworkObjectSpawn> spawns = new List<NetworkObjectSpawn>();
            for (int j = 0; j < no_spawner.SpawnEnemies.Length; j++)
            {
                spawns.Add(new NetworkObjectSpawn
                {
                    Name = no_spawner.SpawnEnemies[j].EnemyPrefabType.ToString(),
                    Chance = no_spawner.SpawnEnemies[j].SpawnChance,
                });
            }

            for (int j = 0; j < no_spawner.SpawnDestructibles.Length; j++)
            {
                spawns.Add(new NetworkObjectSpawn
                {
                    Name = no_spawner.SpawnDestructibles[j].DestructiblePrefabType.ToString(),
                    Chance = no_spawner.SpawnDestructibles[j].SpawnChance,
                });
            }

            for (int j = 0; j < no_spawner.SpawnInteractables.Length; j++)
            {
                spawns.Add(new NetworkObjectSpawn
                {
                    Name = no_spawner.SpawnInteractables[j].InteractablePrefabType.ToString(),
                    Chance = no_spawner.SpawnInteractables[j].SpawnChance,
                });
            }

            for (int j = 0; j < no_spawner.SpawnProps.Length; j++)
            {
                spawns.Add(new NetworkObjectSpawn
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

    private void CreatePlayerSpawnPoints()
    {
        var no_playerSpawnPointsList = new List<PlayerSpawnPoints>(GetComponentsInChildren<PlayerSpawnPoints>());
        if (no_playerSpawnPointsList.Count <= 0) return;

        // we have spawn points so clear the old list
        m_availablePlayerSpawnPoints.Clear();

        foreach (var no_playerSpawnPoints in no_playerSpawnPointsList)
        {
            for (int i = 0; i < no_playerSpawnPoints.transform.childCount; i++)
            {
                m_availablePlayerSpawnPoints.Add(no_playerSpawnPoints.transform.GetChild(i).position);
            }
        }

        // send warning if have less than 3 spawn points in the level
        if (m_availablePlayerSpawnPoints.Count < 3)
        {
            Debug.Log("Warning: Less than 3 player spawn points are available, some players might spawn at (0,0,0)");
        }

        // destroy the spawn points list
        for (int i = 0; i < no_playerSpawnPointsList.Count; i++)
        {
            Destroy(no_playerSpawnPointsList[i].gameObject);
        }
        no_playerSpawnPointsList.Clear();
    }

    public Vector3 PopPlayerSpawnPoint()
    {
        Vector3 spawnPoint = Vector3.zero;
        if (m_availablePlayerSpawnPoints.Count > 0)
        {
            var randIndex = UnityEngine.Random.Range(0, m_availablePlayerSpawnPoints.Count);
            spawnPoint = m_availablePlayerSpawnPoints[randIndex];
            m_availablePlayerSpawnPoints.RemoveAt(randIndex);
        } else
        {
            Debug.Log("NetworkLevel: Ran out of spawn points! Returning Vector3.zero");
        }

        return spawnPoint;
    }

    public void CreateSubLevels()
    {
        var subLevelSpawners = new List<SubLevelSpawner>(GetComponentsInChildren<SubLevelSpawner>());
        for (int i = 0; i < subLevelSpawners.Count; i++)
        {
            var subLevelSpawner = subLevelSpawners[i];

            // normalize the spawn chances
            float sum = 0;
            foreach (var subLevel in subLevelSpawner.SubLevels)
            {
                sum += subLevel.SpawnChance;
            }

            foreach (var subLevel in subLevelSpawner.SubLevels)
            {
                subLevel.SpawnChance /= sum;
            }

            // go through rand chances
            GameObject subLevelPrefab = null;
            var randValue = UnityEngine.Random.Range(0, 0.999f);
            for (int j = 0; j < subLevelSpawner.SubLevels.Count; j++)
            {
                var subLevel = subLevelSpawner.SubLevels[j];
                if (randValue < subLevel.SpawnChance)
                {
                    subLevelPrefab = subLevel.SubLevelPrefab;
                    break;
                }
                else
                {
                    randValue -= subLevel.SpawnChance;
                }
            }

            if (subLevelPrefab != null)
            {
                LevelManager.Instance.LevelSpawningCount++;

                var newSubLevel = Instantiate(subLevelPrefab);
                newSubLevel.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}







// SAVING THIS CODE IN CASE WE SEE ISSUES WITH LOAD TIMES LATER ON AND NEED TO DIRECTLY ASSIGN/ACCESS PREFABS
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