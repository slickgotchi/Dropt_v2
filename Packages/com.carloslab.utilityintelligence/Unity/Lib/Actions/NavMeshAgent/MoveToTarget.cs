﻿using CarlosLab.Common;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence
{
    [Category("NavMeshAgent")]
    public class MoveToTarget : NavMeshActionTask
    {
        public VariableReference<float> Speed = 5;

        protected override void OnStart()
        {
            TaskConsole.Instance.Log($"Agent: {Agent.Name} MoveToTarget OnStart Frame: {Time.frameCount}");

            navMeshAgent.speed = Speed;
            MoveToTarget();
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            TaskConsole.Instance.Log($"Agent: {Agent.Name} MoveToTarget OnUpdate Frame: {Time.frameCount}");

            if (HasArrived())
                return UpdateStatus.Success;
            
            return UpdateStatus.Running;
        }

        protected override void OnEnd()
        {
            TaskConsole.Instance.Log($"Agent: {Agent.Name} MoveToTarget OnEnd Frame: {Time.frameCount}");

            StopMove();
        }
    }
}