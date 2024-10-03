using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Mathematics;
using System.Threading.Tasks;

public class BombSnail_Detonator : NetworkBehaviour
{
    public int detonationTime = 3;
    public TextMeshProUGUI detonationText;

    private NetworkVariable<bool> m_isTriggered = new NetworkVariable<bool>(false);
    private NetworkVariable<int> m_detonationTimer = new NetworkVariable<int>(0);

    private Dropt.EnemyAI m_enemyAI;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        m_detonationTimer.Value = detonationTime;
        detonationText.enabled = false;
        m_enemyAI = GetComponent<Dropt.EnemyAI>();
    }

    private async void Update()
    {
        if (IsServer)
        {
            // check for triggered (aggro) state
            if (m_enemyAI.state.Value == Dropt.EnemyAI.State.Aggro && !m_isTriggered.Value)
            {
                m_isTriggered.Value = true;
                await StartCountDown();
                m_enemyAI.ChangeState(Dropt.EnemyAI.State.Telegraph);
            }
        }

        //if (IsClient)
        //{
        //    detonationText.enabled = m_isTriggered.Value;
        //    detonationText.text = math.ceil(m_detonationTimer.Value).ToString("F0");
        //}
    }

    private async Task StartCountDown()
    {
        Animator animator = GetComponent<Animator>();

        animator.Play("BombSnail_LongFuse");

        while (m_detonationTimer.Value != 0 && m_enemyAI.state.Value == Dropt.EnemyAI.State.Aggro)
        {
            await Task.Delay(1000);
            m_detonationTimer.Value -= 1;
            if (m_detonationTimer.Value == 1)
            {
                animator.Play("BombSnail_ShortFuse");
            }
        }
    }
}
