using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Mathematics;

public class BombSnail_Detonator : NetworkBehaviour
{
    public float detonationTime = 3f;
    public TextMeshProUGUI detonationText;

    private NetworkVariable<bool> m_isTriggered = new NetworkVariable<bool>(false);
    private NetworkVariable<float> m_detonationTimer = new NetworkVariable<float>(0);

    private Dropt.EnemyAI m_enemyAI;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        m_detonationTimer.Value = detonationTime;
        detonationText.enabled = false;
        m_enemyAI = GetComponent<Dropt.EnemyAI>();
    }

    private void Update()
    {
        if (IsServer)
        {
            // check for triggered (aggro) state
            if (m_enemyAI.state.Value == Dropt.EnemyAI.State.Aggro && !m_isTriggered.Value)
            {
                m_isTriggered.Value = true;
                GetComponent<Animator>().Play("BombSnail_LongFuse");
            }

            // if triggered, reduce detonation timer
            if (m_isTriggered.Value)
            {
                m_detonationTimer.Value -= Time.deltaTime;

                if (m_detonationTimer.Value <= 0)
                {
                    // do bombsnails attack (which is to self destruct)
                    m_enemyAI.ChangeState(Dropt.EnemyAI.State.Telegraph);
                }
            }
        }

        if (IsClient)
        {
            detonationText.enabled = m_isTriggered.Value;
            detonationText.text = math.ceil(m_detonationTimer.Value).ToString("F0");
        }
    }
}
