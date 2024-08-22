using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using CarlosLab.UtilityIntelligence;

public class LilEssence : NetworkBehaviour
{
    private UtilityAgentController m_utilityAgentController;
    private Animator m_animator;
    private Decision m_lastDecision;

    private void Awake()
    {
        m_utilityAgentController = GetComponent<UtilityAgentController>();
        m_animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (m_utilityAgentController != null && IsServer)
        {
            var decision = m_utilityAgentController.Intelligence.CurrentDecision;
            if (decision != null)
            {
                if (decision.Name == "Flee" && (m_lastDecision == null || m_lastDecision.Name != decision.Name))
                {
                    m_animator.Play("LilEssence_Alert");
                }
                else if (decision.Name == "Roam")
                {
                    m_animator.Play("LilEssence_Idle");
                }

                m_lastDecision = decision;
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsServer)
        {
            PlayerDungeonData playerData = collision.gameObject.GetComponent<PlayerDungeonData>();
            if (playerData != null)
            {
                Debug.Log("Add 10 essence to player");
                playerData.AddEssence(10);
                gameObject.GetComponent<UtilityAgentFacade>().Destroy();
                //gameObject.GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}
