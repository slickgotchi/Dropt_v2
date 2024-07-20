using CarlosLab.UtilityIntelligence;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dropt.UtilityIntelligence
{
    public class MyIsAlertingInput : Input<bool>
    {
        protected override bool OnGetRawInput(InputContext context)
        {
            var alertObject = AgentFacade.GetComponent<Transform>();

            if (!alertObject.HasComponent<CharacterStatus>()) return false;

            return alertObject.GetComponent<CharacterStatus>()
                .IsAlerting();
        }
    }

}

