
namespace CarlosLab.UtilityIntelligence.Examples
{
    [Category("Examples")]
    public class StartRangedAttack : ActionTask
    {
        public RangedAttackType AttackType;
        public int ConsumeEnergy;
        public int AttackDamage;
        public int ProjectileSpeed;
        public float MaxCurvedHeight;
        
        protected RangedAttacker rangedAttacker;
        private ArcherAnimation archerAnimation;
        
        protected override void OnAwake()
        {
            rangedAttacker = GetComponent<RangedAttacker>();
            archerAnimation = GetComponentInChildren<ArcherAnimation>();
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            StartAttack();
            return UpdateStatus.Success;
        }

        private void StartAttack()
        {
            var attackTarget = TargetGameObject.GetComponent<CharacterAttackTarget>();
            if (AttackType == RangedAttackType.Straight)
            {
                rangedAttacker.StartAttackWithStraightProjectile(attackTarget.StraightProjectileTargetPoint, ConsumeEnergy, AttackDamage, ProjectileSpeed);
            }
            else
            {
                archerAnimation.PlaySpineRotationAnimation();
                rangedAttacker.StartAttackWithCurvedProjectile(attackTarget.CurvedProjectileTargetPoint, ConsumeEnergy, AttackDamage, ProjectileSpeed, MaxCurvedHeight);
            }
        }
    }
}