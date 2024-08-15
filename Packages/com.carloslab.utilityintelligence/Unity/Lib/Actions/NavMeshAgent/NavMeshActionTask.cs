#region

using CarlosLab.Common;
using UnityEngine;
using UnityEngine.AI;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public abstract class NavMeshActionTask : ActionTask
    {
        public VariableReference<NavMeshAgent> NavMeshAgent;

        protected NavMeshAgent navMeshAgent => NavMeshAgent.Value;

        protected float RemainingDistance
        {
            get
            {
                float remainingDistance = float.PositiveInfinity;

                if (navMeshAgent.enabled)
                {
                    if (!navMeshAgent.pathPending)
                        remainingDistance = navMeshAgent.remainingDistance;
                }

                return remainingDistance;
            }
        }
        
        protected override void OnAwake()
        {
            base.OnAwake();
            if (NavMeshAgent.Value == null)
                NavMeshAgent.Value = GetComponentInChildren<NavMeshAgent>();
        }

        protected bool HasArrived()
        {
            bool hasArrived = RemainingDistance <= navMeshAgent.stoppingDistance;
            return hasArrived;
        }

        protected void StartMove(Vector3 destination)
        {
            if (!navMeshAgent.enabled) return;

            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(destination);
        }
        
        protected void MoveToTarget()
        {
            Vector3 targetPosition = TargetTransform.position;
            // targetPosition.y = 0;
            StartMove(targetPosition);
        }

        protected void StopMove()
        {
            if (!navMeshAgent.enabled) return;

            navMeshAgent.isStopped = true;
        }

        protected void SetUpdateRotation(bool update)
        {
            navMeshAgent.updateRotation = update;
            navMeshAgent.updateUpAxis = update;
        }
    }
}