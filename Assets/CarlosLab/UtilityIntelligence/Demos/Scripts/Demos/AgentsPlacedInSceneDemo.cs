#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence.Demos
{
    public class AgentsPlacedInSceneDemo : MonoBehaviour
    {
        [SerializeField]
        private UtilityWorldController world;

        [SerializeField]
        private List<UtilityAgentController> agents;

        [SerializeField]
        private List<UtilityEntityController> chargeStations;

        private void Start()
        {
            foreach (UtilityAgentController agent in agents)
            {
                agent.Register(world);
            }

            foreach (UtilityEntityController chargeStation in chargeStations)
            {
                chargeStation.Register(world);
            }
        }
    }
}