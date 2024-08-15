using CarlosLab.Common;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Examples")]
    public class StartMeleeAttack : ActionTask
    {
        public MeleeAttackType AttackType;
        public int AttackDamage;
        public int AttackForce;
        public int ConsumeEnergy;
        
        public VariableReference<float> AttackRange;
        public VariableReference<int> AttackNumber;
        public VariableReference<string> AttackAnimationName;

        protected MeleeAttacker attacker;
        protected override void OnAwake()
        {
            attacker = GetComponent<MeleeAttacker>();
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            AttackNumber.Value++;
            if (AttackNumber.Value > 4)
                AttackNumber.Value = 1;

            AttackAnimationName.Value = "Attack" + AttackNumber.Value;
            
            StartAttack(TargetGameObject, ConsumeEnergy, AttackDamage, AttackRange);
            return UpdateStatus.Success;
        }

        private void StartAttack(GameObject target, int consumeEnergy, int attackDamage, float attackRange)
        {
            if (AttackType == MeleeAttackType.AttackWithoutForce)
            {
                attacker.StartAttackWithoutForce(target, consumeEnergy, attackDamage, attackRange);
            }
            else
            {
                attacker.StartAttackWithForce(target, consumeEnergy, attackDamage, attackRange, AttackForce);
            }
        }
    }
}