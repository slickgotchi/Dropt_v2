using Dialogue.View;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Dialogue.Components
{
    public class NextBark : MonoBehaviour
    {
        [SerializeField] private BarkStarter m_barkStarter;
        [SerializeField] private DialogueActor m_actor;
        private AbstractBarkUI RawView => m_actor.barkUISettings.barkUI;

        private NpcBarkerView m_View;

        private void Start()
        {
            if (RawView is NpcBarkerView view)
            {
                m_View = view;

                m_View.OnSkipClick += Next;
            }
        }

        private void OnDestroy()
        {
            if (m_View == null)
            {
                return;
            }

            m_View.OnSkipClick -= Next;
        }

        private void Next()
        {
            if (!m_View.isPlaying || m_View.Typewriter.isPlaying)
            {
                return;
            }
            
            m_barkStarter.TryBark(null);
        }
    }
}