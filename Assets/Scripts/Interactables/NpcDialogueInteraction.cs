using AI.NPC;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Interactables
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(DialogueActor))]
    [RequireComponent(typeof(NpcMover))]
    [RequireComponent(typeof(BarkTrigger))]
    public class NpcDialogueInteraction : Interactable
    {
        [SerializeField] private DialogueActor m_actor;
        [SerializeField] private float m_distanceToHide;
        [SerializeField] private NpcMover m_mover;
        [SerializeField] private BarkTrigger m_barkTrigger;

        private Collider2D m_collider;
        private Transform m_player;

        private AbstractBarkUI View => m_actor.barkUISettings.barkUI;

        private GameObject Player
        {
            get
            {
                if (m_player == null)
                {
                    m_player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].transform;
                }

                return m_player.gameObject;
            }
        }

        private bool IsOutOfArea => Vector2.Distance(GetPlayerPosition(), m_actor.transform.position) > m_distanceToHide;

        private void Start()
        {
            m_barkTrigger.trigger = DialogueTriggerEvent.OnUse;
            m_collider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (m_collider.enabled)
            {
                return;
            }

            if (IsOutOfArea)
            {
                if (View == null)
                {
                    return;
                }

                View.Hide();
                m_mover.Resume();
                m_collider.enabled = true;
            }
        }

        public override void OnTriggerEnter2DInteraction()
        {
            base.OnTriggerEnter2DInteraction();
            
            m_mover.Stop();

            m_mover.FaceTo(Player.transform);

            m_barkTrigger.OnUse();
            
            m_collider.enabled = false;
        }

        private Vector2 GetPlayerPosition()
        {
            return Player.transform.position;
        }
    }
}