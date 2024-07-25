using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;



public class EscapePortal : Interactable
{
    private float m_fHoldTimer = 0;
    private float k_fHoldtime = 0.5f;
    private float m_nextLevelCooldownTimer = 0;
    private float k_nextLevelCooldown = 3;

    public override void OnStartInteraction()
    {
        InteractableUICanvas.Instance.InteractTextbox.SetActive(true);
    }

    public override void OnUpdateInteraction()
    {
        m_nextLevelCooldownTimer -= Time.deltaTime;
        InteractableUICanvas.Instance.InteractSlider.value = m_fHoldTimer / k_fHoldtime;

        if (Input.GetKey(KeyCode.F))
        {
            m_fHoldTimer += Time.deltaTime;
            if (m_fHoldTimer >= k_fHoldtime && m_nextLevelCooldownTimer <= 0)
            {
                TryGoToDegenapeVillageLevelServerRpc();
                m_nextLevelCooldownTimer = k_nextLevelCooldown;
            }
        }
        else
        {
            m_fHoldTimer = 0;
        }
    }

    public override void OnFinishInteraction()
    {
        InteractableUICanvas.Instance.InteractTextbox.SetActive(false);
    }

    [Rpc(SendTo.Server)]
    void TryGoToDegenapeVillageLevelServerRpc()
    {
        if (m_nextLevelCooldownTimer > 0) return;

        LevelManager.Instance.GoToDegenapeVillageLevel();
        m_nextLevelCooldownTimer = k_nextLevelCooldown;
    }
}
