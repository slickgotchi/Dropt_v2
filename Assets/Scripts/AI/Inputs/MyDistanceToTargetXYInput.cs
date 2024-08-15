using UnityEngine;
using CarlosLab.UtilityIntelligence;

namespace CarlosLab.UtilityIntelligence
{
    public class MyDistanceToTargetXYInput : Input<float>
    {
        protected override float OnGetRawInput(in InputContext context)
        {
            var myPosition = AgentFacade.Position;
            var targetPosition = context.TargetFacade.Position;
            myPosition.Z = 0;
            targetPosition.Z = 0;

            return Vector3.Distance(myPosition, targetPosition);
        }
    }
}