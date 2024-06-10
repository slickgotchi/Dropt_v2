using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObjectSpawner : MonoBehaviour
{
    public float NoSpawnChance = 0;

    //public SpawnGameObject[] SpawnPrefabs;
    public EnemySpawnPrefab[] SpawnEnemies;
    public DestructibleSpawnPrefab[] SpawnDestructibles;
    public InteractableSpawnPrefab[] SpawnInteractables;
    public PropSpawnPrefab[] SpawnProps;

    private List<Transform> _spawnPoints;
}

public enum EnemySpawnType
{
    BombSnail,
    FudSpirit,
    FudWisp,
    FussPot,
    GasBag,
    GeodeShade,
    LeafShade,
    SentryBot,
    Snail,
    Spider,
    SpiderPod,
    WispHollow,
}

public enum DestructibleSpawnType
{
    BlueCrate,
    BlueFern,
    Geode_Plain,
    Geode_Common,
    Geode_Uncommon,
    Geode_Rare,
    Geode_Legendary,
    Geode_Mythical,
    Geode_Godlike,
}

public enum InteractableSpawnType
{
    Hole,
    GotchiSelectPortal,
    EscapePortal,
}

public enum PropSpawnType
{
    LightPole,
    RuinBlock,
}


[System.Serializable]
public class EnemySpawnPrefab
{
    public EnemySpawnType EnemyPrefabType;
    public float SpawnChance;
}

[System.Serializable]
public class DestructibleSpawnPrefab
{
    public DestructibleSpawnType DestructiblePrefabType;
    public float SpawnChance;
}

[System.Serializable]
public class InteractableSpawnPrefab
{
    public InteractableSpawnType InteractablePrefabType;
    public float SpawnChance;
}

[System.Serializable]
public class PropSpawnPrefab
{
    public PropSpawnType PropPrefabType;
    public float SpawnChance;
}