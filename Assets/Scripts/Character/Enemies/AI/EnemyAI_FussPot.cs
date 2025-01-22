using UnityEngine;
using Unity.Netcode;

namespace Dropt
{
    public class EnemyAI_FussPot : EnemyAI
    {
        [Header("FussPot Specific")]
        public float ProjectileSpreadInDegrees = 30;

        private Animator m_animator;
        private EnemyController m_enemyController;
        private SoundFX_Fusspot m_soundFX_Fusspot;

        [SerializeField] private FussPot_Erupt m_fussPot_Erupt;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
            m_enemyController = GetComponent<EnemyController>();
            m_soundFX_Fusspot = GetComponent<SoundFX_Fusspot>();
        }

        public override void OnSpawnStart()
        {
            base.OnSpawnStart();
        }

        public override void OnRoamStart()
        {
            base.OnRoamStart();
            if (!IsServer)
            {
                return;
            }

            Utils.Anim.Play(m_animator, "Fusspot_Idle");
        }

        public override void OnTelegraphStart()
        {
            if (!IsServer)
            {
                return;
            }
            // set our facing direction
            m_enemyController.SetFacingFromDirection(AttackDirection, TelegraphDuration);
            Utils.Anim.PlayAnimationWithDuration(m_animator, "Fusspot_Anticipation", TelegraphDuration);
            m_soundFX_Fusspot.PlayChargeSound();
        }

        public override void OnTelegraphFinish()
        {
        }

        public override void OnRoamUpdate(float dt)
        {
        }

        public override void OnAggroUpdate(float dt)
        {
        }

        public override void OnAttackStart()
        {
            SimpleFussPotAttack();
            Utils.Anim.PlayAnimationWithDuration(m_animator, "Fusspot_Fire", AttackDuration);
            m_soundFX_Fusspot.PlayAttackSound();
            m_enemyController.SetFacingFromDirection(AttackDirection, AttackDuration);
        }

        public override void OnAttackFinish()
        {
            base.OnAttackFinish();
            Utils.Anim.Play(m_animator, "Fusspot_Idle");
        }

        // attack
        protected void SimpleFussPotAttack()
        {
            // check we have a primary attack.
            //if (PrimaryAttack == null) return;

            //// instantiate an attack
            //GameObject ability = Instantiate(PrimaryAttack);

            //// get enemy ability of attack
            //EnemyAbility enemyAbility = ability.GetComponent<EnemyAbility>();
            //if (enemyAbility == null) return;

            //// get fusspot erupt ability
            ////FussPot_Erupt fussPotErupt = ability.GetComponent<FussPot_Erupt>();
            ////fussPotErupt.ProjectileSpreadInDegrees = ProjectileSpreadInDegrees;

            //// initialise the ability
            //ability.GetComponent<NetworkObject>().Spawn();
            //enemyAbility.Activate(gameObject, NearestPlayer, AttackDirection, AttackDuration, PositionToAttack);

            m_fussPot_Erupt.Activate(gameObject, NearestPlayer, AttackDirection, AttackDuration, PositionToAttack);
        }
    }
}