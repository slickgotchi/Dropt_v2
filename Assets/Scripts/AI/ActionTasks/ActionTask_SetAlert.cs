using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarlosLab.UtilityIntelligence;
using CarlosLab.Common;
using UnityEngine.AI;
using Unity.Mathematics;
using Unity.VisualScripting;

public class ActionTask_SetAlert : ActionTask
{
    public VariableReference<bool> isAlert = true;

    protected override UpdateStatus OnUpdate(float deltaTime)
    {
        // ensure we still have a valid context and target
        if (Context == null || Context.Target == null) return UpdateStatus.Running;

        if (!GameObject.HasComponent<CharacterStatus>())
        {
            GameObject.AddComponent<CharacterStatus>();
        } 

        GameObject.GetComponent<CharacterStatus>().SetAlerting(isAlert);

        return UpdateStatus.Success;
    }
}
