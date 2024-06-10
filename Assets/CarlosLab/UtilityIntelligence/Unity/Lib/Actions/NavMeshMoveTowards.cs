#region

using CarlosLab.Common;
using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public class NavMeshMoveTowards : NavMeshActionTask
    {
        public VariableReference<float> Speed;

        protected override void OnStart()
        {
            NavMeshAgent.speed = Speed;
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            Vector3 targetPosition = Context.Target.GetComponent<Transform>().position;
            targetPosition.y = 0;
            StartMove(targetPosition);

            if (HasArrived())
                return UpdateStatus.Success;
            return UpdateStatus.Running;
        }

        protected override void OnEnd()
        {
            StopMove();
        }
    }
}