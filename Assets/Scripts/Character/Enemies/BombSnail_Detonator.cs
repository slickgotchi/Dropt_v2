using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Mathematics;

public class BombSnail_Detonator : NetworkBehaviour
{
    public float detonationTime = 3f;
    public TextMeshProUGUI detonationText;

    private NetworkVariable<bool> m_isTriggered = new NetworkVariable<bool>(false);
    private NetworkVariable<float> m_detonationTimer = new NetworkVariable<float>(0);

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        m_detonationTimer.Value = detonationTime;
        detonationText.enabled = false;
    }

    private void Update()
    {

        if (IsServer)
        {
            // check for triggered (aggro) state
            if (GetComponent<Dropt.EnemyAI>().state == Dropt.EnemyAI.State.Aggro)
            {
                m_isTriggered.Value = true;
            }

            // if triggered, reduce detonation timer
            if (m_isTriggered.Value)
            {
                m_detonationTimer.Value -= Time.deltaTime;

                if (m_detonationTimer.Value <= 0)
                {
                    // do bombsnails attack (which is to self destruct)
                    Debug.Log("Set state to attack");
                    GetComponent<Dropt.EnemyAI>().state = Dropt.EnemyAI.State.Telegraph;
                }
            }
        }

        if (IsClient)
        {
            if (m_isTriggered.Value)
            {
                detonationText.enabled = true;

                detonationText.text = math.ceil(m_detonationTimer.Value).ToString("F0");
            }
        }
    }
}
