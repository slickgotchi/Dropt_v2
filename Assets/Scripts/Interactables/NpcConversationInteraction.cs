using AI.NPC;
using Dialogue.View;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Interactables
{
    [RequireComponent(typeof(NpcMover))]
    [RequireComponent(typeof(DialogueSystemEvents))]
    public class NpcConversationInteraction : Interactable
    {
        [SerializeField] private NpcMover m_mover;

        [SerializeField] private NpcDialogueView _viewPrefab;

        [SerializeField] private DialogueSystemEvents _conversationEvents;

        [SerializeField] private Transform m_actor;

        [SerializeField] [ConversationPopup(true)]
        private string m_conversation;

        private PlayerController m_player;

        private PlayerController Player
        {
            get
            {
                if (m_player == null)
                {
                    m_player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId]
                        .GetComponent<PlayerController>();
                }

                return m_player;
            }
        }

        private PlayerHUDCanvas PlayerHUD => PlayerHUDCanvas.Instance;
        private bool IsConversationActive => DialogueManager.isConversationActive;
        private PlayerPrediction PlayerPrediction => Player.GetComponent<PlayerPrediction>();
        protected virtual bool ShouldHideHUD { get; } = true;
        protected virtual bool ShouldStopPlayerMove { get; } = true;

        public override void OnTriggerEnter2DInteraction()
        {
            base.OnTriggerEnter2DInteraction();

            if (IsConversationActive)
            {
                return;
            }

            SetActiveTextBox(true);
            m_mover.FaceTo(Player.transform);
            m_mover.Stop();
        }

        public override void OnTriggerUpdateInteraction()
        {
            base.OnTriggerUpdateInteraction();

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (IsConversationActive)
                {
                    return;
                }

                OnTriggerExit2DInteraction();
                StartConversation();
            }
        }

        private void StartConversation()
        {
            _conversationEvents.conversationEvents.onConversationEnd.AddListener(OnConversationEnded);

            if (ShouldStopPlayerMove)
            {
                PlayerPrediction.IsInputEnabled = false;
            }

            if (ShouldHideHUD)
            {
                PlayerHUD.Hide();
            }

            m_mover.Stop();

            DialogueManager.StartConversation(m_conversation, m_actor, null, 0, _viewPrefab);
        }

        protected virtual void OnConversationEnded(Transform actor)
        {
            _conversationEvents.conversationEvents.onConversationEnd.RemoveListener(OnConversationEnded);

            if (ShouldStopPlayerMove)
            {
                PlayerPrediction.IsInputEnabled = true;
            }

            if (ShouldHideHUD)
            {
                PlayerHUD.Show();
                m_mover.Resume();
            }
        }

        public override void OnTriggerExit2DInteraction()
        {
            base.OnTriggerExit2DInteraction();

            SetActiveTextBox(false);

            if (DialogueManager.isConversationActive)
            {
                return;
            }

            m_mover.Resume();
        }

        private void SetActiveTextBox(bool value)
        {
            InteractableUICanvas.Instance.InteractTextbox.SetActive(value);
        }
    }
}