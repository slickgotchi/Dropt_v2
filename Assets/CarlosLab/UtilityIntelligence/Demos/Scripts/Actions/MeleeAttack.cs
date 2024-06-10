#region

using CarlosLab.Common;
using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence.Demos
{
    public class MeleeAttack : NavMeshActionTask
    {
        #region Fields and Properties

        public VariableReference<float> AttackForce = 10;
        public VariableReference<int> ConsumeEnergy;

        public float AttackSpeed = 20;
        public int AttackDamage = 10;

        private bool isAttacking;
        private float originSpeed;
        
        #endregion

        #region Lifecycle Functions

        protected override void OnStart()
        {
            isAttacking = false;
            originSpeed = NavMeshAgent.speed;
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            if (!(Context.Target is UtilityAgent targetAgent)) return UpdateStatus.Failure;
            
            Character myCharacter = GetComponent<Character>();
            Character targetCharacter = targetAgent.GetComponent<Character>();

            if (!isAttacking)
            {
                if (myCharacter.State == CharacterState.Normal && NavMeshAgent.enabled)
                {
                    CharacterEnergy characterEnergy = GetComponent<CharacterEnergy>();

                    if (characterEnergy.Energy < ConsumeEnergy) 
                        return UpdateStatus.Failure;
                
                    characterEnergy.Energy -= ConsumeEnergy;
                    StartAttack(targetCharacter);
                    isAttacking = true;
                }
            }
            else
            {
                Vector3 vector = targetCharacter.transform.position - Transform.position;
                vector.y = 0;

                float distanceSqr = vector.sqrMagnitude;
                Vector3 direction = vector.normalized;

                if (distanceSqr <= 2.0f)
                {
                    targetCharacter.OnAttacked(AttackDamage, AttackForce, direction);
                
                    return UpdateStatus.Success;
                }
                else if (myCharacter.State == CharacterState.Attacked || HasArrived())
                {
                    return UpdateStatus.Failure;
                }
            }
                
            return UpdateStatus.Running;
        }
        
        
        private void StartAttack(Character targetCharacter)
        {
            NavMeshAgent.speed = AttackSpeed;
            Vector3 targetPosition = targetCharacter.transform.position;
            targetPosition.y = 0;
            StartMove(targetPosition);
        }

        protected override void OnEnd()
        {
            NavMeshAgent.speed = originSpeed;
            StopMove();
        }

        #endregion
    }
}