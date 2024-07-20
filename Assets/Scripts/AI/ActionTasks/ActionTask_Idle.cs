using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarlosLab.UtilityIntelligence;

public class ActionTask_Idle : ActionTask
{
    protected override UpdateStatus OnUpdate(float deltaTime)
    {
        return UpdateStatus.Running;
    }
}
