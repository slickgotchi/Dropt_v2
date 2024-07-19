using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObjectSpawner : MonoBehaviour
{
    public enum ActivationType { ElapsedTime, OtherSpawnerCleared }

    [Header("Activation Setup")]
    public ActivationType activationType = ActivationType.ElapsedTime;

    public float activateOnElapsedTime = 0f;
    public NetworkObjectSpawner activateOnOtherSpawnerCleared;

    [Header("Spawn Parameters")]
    public float NoSpawnChance = 0;

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
    DO_NOT_USE_BlueCrate,
    DO_NOT_USE_BlueFern,
    Geode_Plain,
    Geode_Common,
    Geode_Uncommon,
    Geode_Rare,
    Geode_Legendary,
    Geode_Mythical,
    Geode_Godlike,
    Rock_Large,
    Fern_FUD,
    Fern_FOMO,
    Fern_ALPHA,
    Fern_KEK,
    Mangrove,
    Mangrove_AP,
    Mangrove_HP,
    Flower_FUD,
    Flower_FOMO,
    Flower_ALPHA,
    Flower_KEK,
    FoxyTails,
    Barrel_FUD,
    Barrel_FOMO,
    Barrel_ALPHA,
    Barrel_KEK,
    Crate_Blue,
    Crate_Kinship,
    Chest_Plain,
    Chest_Common,
    Chest_Uncommon,
    Chest_Rare,
    Chest_Legendary,
    Chest_Mythical,
    Chest_Godlike,
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