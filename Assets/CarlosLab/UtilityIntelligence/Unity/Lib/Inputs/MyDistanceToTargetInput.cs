#region

using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public class MyDistanceToTargetInput : Input<float>
    {
        protected override float OnGetRawInput(InputContext context)
        {
            var myPosition = AgentFacade.Position;
            var targetPosition = context.TargetFacade.Position;
            myPosition.Y = 0;
            targetPosition.Y = 0;

            return Vector3.Distance(myPosition, targetPosition);
        }
    }
}