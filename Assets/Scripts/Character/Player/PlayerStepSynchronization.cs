using UnityEngine;

namespace Character.Player
{
    [RequireComponent(typeof(PlayerPrediction))]
    public sealed class PlayerStepSynchronization : MonoBehaviour
    {
        [SerializeField] private int m_MaxWaitingTimeInMs = 500;

        private PlayerPrediction m_playerPrediction;
        private int m_waitingTick;

        private void Awake()
        {
            m_playerPrediction = GetComponent<PlayerPrediction>();
        }

        private void Update()
        {
            if (!m_playerPrediction.IsInputDisabled)
                return;

            var offset = m_playerPrediction.Timer.CurrentTick - m_waitingTick;

            if (m_playerPrediction.LastServerState.tick >= m_waitingTick || offset - m_waitingTick >= m_MaxWaitingTimeInMs)
            {
                m_playerPrediction.IsInputDisabled = false;
            }
        }

        public void WaitUntilReceiveServerData(float durationOffset)
        {
            if (m_playerPrediction.IsInputDisabled)
                return;

            m_waitingTick = m_playerPrediction.Timer.CurrentTick + Mathf.CeilToInt(durationOffset / PlayerPrediction.k_serverTickRate);
            m_playerPrediction.IsInputDisabled = true;
        }
    }
}