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
            m_fussPot_Erupt.Activate(gameObject, NearestPlayer, AttackDirection, AttackDuration, PositionToAttack);
            Utils.Anim.PlayAnimationWithDuration(m_animator, "Fusspot_Fire", AttackDuration);
            m_soundFX_Fusspot.PlayAttackSound();
            m_enemyController.SetFacingFromDirection(AttackDirection, AttackDuration);
        }

        public override void OnAttackFinish()
        {
            base.OnAttackFinish();
            Utils.Anim.Play(m_animator, "Fusspot_Idle");
        }
    }
}