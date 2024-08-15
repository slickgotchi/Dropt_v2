using CarlosLab.Common;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence
{
    [Category("3D")]
    public class MoveTowardsTarget : ActionTask
    {
        public VariableReference<float> Speed = 5;
        public float StoppingDistance = 0.5f;

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            UtilityIntelligenceConsole.Instance.Log(
                $"Entity: {Agent.Name} Target: {Context.Target.Name} MoveToTarget");
            Transform transform = GetComponent<Transform>();
            Vector3 myPosition = transform.position;
            Vector3 targetPosition = TargetTransform.position;

            if (Vector3.Distance(myPosition, targetPosition) <= StoppingDistance)
                return UpdateStatus.Success;

            transform.position = Vector3.MoveTowards(myPosition, targetPosition, Speed * Time.deltaTime);

            return UpdateStatus.Running;
        }
    }
}