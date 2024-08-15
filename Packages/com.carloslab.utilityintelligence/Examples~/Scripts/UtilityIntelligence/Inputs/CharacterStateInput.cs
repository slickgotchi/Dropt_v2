﻿using CarlosLab.Common;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Examples")]
    public class CharacterStateInput : InputFromSource<CharacterState>
    {
        protected override CharacterState OnGetRawInput(in InputContext context)
        {

            UtilityEntity inputSource = GetInputSource(in context);
            if (inputSource.EntityFacade is Character character)
            {
                //Debug.Log($"Agent: {Agent.Name} CharacterStateInput Decision: {context.DecisionName} State: {character.State} Frame: {Time.frameCount}");

                return character.State;
            }

            return CharacterState.Normal;
        }
    }
}