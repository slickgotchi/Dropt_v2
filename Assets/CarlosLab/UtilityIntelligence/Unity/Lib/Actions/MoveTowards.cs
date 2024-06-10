#region

using CarlosLab.Common;
using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public class MoveTowards : ActionTask
    {
        public VariableReference<float> Speed;

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            UtilityIntelligenceConsole.Instance.Log(
                $"Entity: {Agent.Name} Target: {Context.Target.Name} MoveToTarget");
            Transform transform = GetComponent<Transform>();
            Vector3 myPosition = transform.position;
            Vector3 targetPosition = Context.Target.GetComponent<Transform>().position;

            if (Vector3.Distance(myPosition, targetPosition) <= 0.6f)
                return UpdateStatus.Success;

            transform.position = Vector3.MoveTowards(myPosition, targetPosition, Speed * Time.deltaTime);

            return UpdateStatus.Running;
        }
    }
}