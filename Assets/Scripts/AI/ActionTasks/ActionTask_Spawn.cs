using CarlosLab.Common;
using CarlosLab.UtilityIntelligence;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionTask_Spawn : ActionTask
{
    public VariableReference<float> SpawnTime = 1f;

    private float m_spawnTimer = 0f;

    protected override UpdateStatus OnUpdate(float deltaTime)
    {
        m_spawnTimer += deltaTime;

        if (m_spawnTimer >= SpawnTime)
        {
            return UpdateStatus.Success;
        }

        return UpdateStatus.Running;
    }
}
