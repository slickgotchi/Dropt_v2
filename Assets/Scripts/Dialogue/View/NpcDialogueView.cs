using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;
using StandardDialogueUI = PixelCrushers.DialogueSystem.Wrappers.StandardDialogueUI;

namespace Dialogue.View
{
    public class NpcDialogueView : StandardDialogueUI
    {
        [SerializeField] private Button m_exitButton;
        
        public override void Start()
        {
            base.Start();
            
            m_exitButton.onClick.AddListener(OnExitClicked);
        }

        private void OnDestroy()
        {
            m_exitButton.onClick.RemoveListener(OnExitClicked);
        }

        private void OnExitClicked()
        {
            DialogueManager.StopConversation();
        }
    }
}