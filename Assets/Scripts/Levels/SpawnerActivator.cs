using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnerActivator
{
    public enum ActivationType { Null, ElapsedTime, OtherSpawnerCleared, IsActivated }
    public ActivationType Type;

    public float ActivateAfterTime = 0f;
    public SpawnerActivator otherSpawnerActivator;

    public List<GameObject> spawnedObjects = new List<GameObject>();

    private float m_elapsedTime = 0;

    public void Update(float dt)
    {
        m_elapsedTime += dt;

        if (Type == ActivationType.ElapsedTime)
        {
            if (m_elapsedTime > ActivateAfterTime)
            {
                ActivateAll();
                Type = ActivationType.IsActivated;
            }
        }

        if (Type == ActivationType.OtherSpawnerCleared)
        {
            if (otherSpawnerActivator == null || otherSpawnerActivator.Type != ActivationType.IsActivated) return;

            bool isOtherSpawnerObjectsActive = false;
            for (int i = 0; i < otherSpawnerActivator.spawnedObjects.Count; i++)
            {
                var otherSpawnedObject = otherSpawnerActivator.spawnedObjects[i];
                if (otherSpawnedObject != null && otherSpawnedObject.activeSelf)
                {
                    isOtherSpawnerObjectsActive = true;
                    break;
                }
            }

            if (!isOtherSpawnerObjectsActive)
            {
                ActivateAll();
                Type = ActivationType.IsActivated;
            }
        }
    }

    void ActivateAll()
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i] == null)
            {
                Debug.LogWarning("SpawnerActivator: Tried to spawn a null object");
            }
            spawnedObjects[i].SetActive(true);
            spawnedObjects[i].GetComponent<NetworkObject>().Spawn();
        }
    }
}
