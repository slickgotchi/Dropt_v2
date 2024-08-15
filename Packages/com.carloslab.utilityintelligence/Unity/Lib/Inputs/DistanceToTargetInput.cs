#region

using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public class DistanceToTargetInput : Input<float>
    {
        protected override float OnGetRawInput(in InputContext context)
        {
            var currentPos = AgentFacade.Position;
            var targetPos = context.TargetFacade.Position;
            currentPos.Y = 0;
            targetPos.Y = 0;

            // float result = 0;
            // for (int i = 1; i < 100; i++)
            // {
            //     result += Mathf.Sqrt(i) * Mathf.Sin(i) * Mathf.Cos(i);
            // }
            
            return Vector3.Distance(currentPos, targetPos);
        }
    }
}