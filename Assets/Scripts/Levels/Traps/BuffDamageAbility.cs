using UnityEngine;

namespace Level.Traps
{
    public sealed class BuffDamageAbility : MonoBehaviour
    {
        [SerializeField] private BuffObject m_buff;
        [SerializeField] private float m_buffEffectDuration = 1;

        private NetworkCharacter m_networkCharacter;
        private float m_timer = 0f;

        public float BuffEffectDuration => m_buffEffectDuration;

        public void Damage(NetworkCharacter character)
        {
            m_timer = m_buffEffectDuration;
            m_networkCharacter = character;

            character.AddBuffObject(m_buff);
            enabled = true;
        }

        private void EndBuff()
        {
            m_networkCharacter?.RemoveBuffObject(m_buff);
            enabled = false;
        }

        // Update is called once per frame
        private void Update()
        {
            m_timer -= Time.deltaTime;

            if (m_timer < 0)
            {
                EndBuff();
            }
        }
    }
}