using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;



public class EscapePortal : Interactable
{
    //private float m_fHoldTimer = 0;
    //private float k_fHoldtime = 0.5f;
    //private float m_nextLevelCooldownTimer = 0;
    //private float k_nextLevelCooldown = 3;

    public override void OnHoldFinishInteraction()
    {
        GoToDegenapeVillageLevelServerRpc();
    }

    [Rpc(SendTo.Server)]
    void GoToDegenapeVillageLevelServerRpc()
    {
        LevelManager.Instance.GoToDegenapeVillageLevel();

    }
}
