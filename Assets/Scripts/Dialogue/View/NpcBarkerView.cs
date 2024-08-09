using System;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Dialogue.View
{
    public class NpcBarkerView : StandardBarkUI
    {
        [SerializeField] private UnityUITypewriterEffect m_typewriter;
        
        public event Action OnSkipClick;
        public AbstractTypewriterEffect Typewriter => m_typewriter;


        protected override void Update()
        {
            base.Update();

            if (InputDeviceManager.DefaultGetMouseButtonDown(0))
            {
                OnSkipClicked();
            }
        }

        private void OnSkipClicked()
        {
            OnSkipClick?.Invoke();
        }

        public void Skip()
        {
            if (m_typewriter.isPlaying)
            {
                m_typewriter.Stop();
            }
        }
    }
}