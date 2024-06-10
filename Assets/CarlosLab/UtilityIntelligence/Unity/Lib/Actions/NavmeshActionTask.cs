#region

using UnityEngine;
using UnityEngine.AI;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public abstract class NavMeshActionTask : ActionTask
    {
        private NavMeshAgent navmeshAgent;

        protected NavMeshAgent NavMeshAgent => navmeshAgent;

        protected float RemainingDistance
        {
            get
            {
                float remainingDistance = float.PositiveInfinity;

                if (navmeshAgent.enabled)
                {
                    if (!navmeshAgent.pathPending)
                        remainingDistance = navmeshAgent.remainingDistance;
                }

                return remainingDistance;
            }
        }

        protected override void OnAwake()
        {
            navmeshAgent = GetComponent<NavMeshAgent>();
        }

        protected bool HasArrived()
        {
            bool hasArrived = RemainingDistance <= navmeshAgent.stoppingDistance;
            return hasArrived;
        }

        protected void StartMove(Vector3 destination)
        {
            if (!navmeshAgent.enabled) return;

            navmeshAgent.isStopped = false;
            navmeshAgent.SetDestination(destination);
        }

        protected void StopMove()
        {
            if (!navmeshAgent.enabled) return;

            navmeshAgent.isStopped = true;
        }
    }
}