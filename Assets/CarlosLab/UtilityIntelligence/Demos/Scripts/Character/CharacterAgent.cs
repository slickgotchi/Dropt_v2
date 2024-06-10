#region

using UnityEngine;
using UnityEngine.AI;

#endregion

namespace CarlosLab.UtilityIntelligence.Demos
{
    public class CharacterAgent : MonoBehaviour
    {
        private NavMeshAgent _agent;

        private void Awake()
        {
            _agent = GetComponentInChildren<NavMeshAgent>();
        }

        public void StartMove(Vector3 destination)
        {
            _agent.isStopped = false;
            _agent.SetDestination(destination);
        }

        public void StopMove()
        {
            _agent.isStopped = true;
        }

        public bool HasArrived()
        {
            float remainingDistance = 0;
            if (_agent.pathPending)
                remainingDistance = float.PositiveInfinity;
            else
                remainingDistance = _agent.remainingDistance;

            return remainingDistance <= _agent.stoppingDistance;
        }
    }
}