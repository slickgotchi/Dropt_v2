using CarlosLab.Common;
using UnityEngine.AI;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Examples")]
    public class UpdateSpeedForever : SetParam
    {
        public VariableReference<NavMeshAgent> NavMeshAgent;


        protected override void OnAwake()
        {
            base.OnAwake();
            
            if (NavMeshAgent.Value == null)
                NavMeshAgent.Value = GetComponentInChildren<NavMeshAgent>();
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            float currentSpeed = NavMeshAgent.Value.velocity.magnitude;
            animator.SetFloat(ParamName, currentSpeed);
            return UpdateStatus.Running;
        }

        protected override void OnEnd()
        {
            animator.SetFloat(ParamName, 0);
        }
    }
}