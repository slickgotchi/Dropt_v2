using System.Collections.Generic;
using CarlosLab.Common;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class PatrolWaypoints : MonoBehaviour
    {
        public List<Transform> Waypoints;
        
        private void Start()
        {
            Character character = GetComponent<Character>();
            var blackboard = character.Entity.Intelligence.Blackboard;
            var waypointsVariable = blackboard.GetVariable<TransformListVariable>(BlackboardVariableNames.Waypoints);
            waypointsVariable.Value = Waypoints;
        }
    }
}
