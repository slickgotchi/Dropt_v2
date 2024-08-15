using UnityEngine.AI;

namespace CarlosLab.Common
{
    public class NavMeshAgentVariable : Variable<NavMeshAgent>
    {
        public static implicit operator NavMeshAgentVariable(NavMeshAgent value)
        {
            return new NavMeshAgentVariable { Value = value };
        }
    }
}