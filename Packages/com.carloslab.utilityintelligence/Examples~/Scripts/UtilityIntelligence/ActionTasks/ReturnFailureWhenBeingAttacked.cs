using System;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Examples")]
    public class ReturnFailureWhenBeingAttacked : ActionTask
    {
        private Character character;
        protected override void OnAwake()
        {
            character = GetComponent<Character>();
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            if (character.State == CharacterState.Attacked)
                return UpdateStatus.Failure;
            else return UpdateStatus.Running;
        }
    }
}