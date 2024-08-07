using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerStatusEffects : NetworkBehaviour
{
    public GameObject Rooted;
    public GameObject Stunned;
    public GameObject Blind;

    public BuffObject rootedBuff;

    private CharacterStatus m_characterStatus;

    public enum Effect
    {
        None,
        Rooted,
        Stunned,
        Blind
    }

    bool m_isRootedStart = false;


    private float m_effectTimer = 0f;

    private void Awake()
    {
        m_characterStatus = GetComponent<CharacterStatus>();
        Rooted.SetActive(false);
    }

    //public void SetVisualEffect(Effect effect, float duration)
    //{
    //    if (IsClient || IsHost)
    //    {
    //        m_effectTimer = duration;

    //        switch (effect)
    //        {
    //            case Effect.Rooted:
    //                DisableAllEffects();
    //                Rooted.SetActive(true);
    //                break;
    //            default: break;
    //        }
    //    }
    //    else
    //    {
    //        SetVisualEffectClientRpc(effect, duration);
    //    }
    //}


    //[Rpc(SendTo.ClientsAndHost)]
    //private void SetVisualEffectClientRpc(Effect effect, float duration)
    //{
    //    SetVisualEffect(effect, duration);
    //}


    private void Update()
    {
        //m_effectTimer -= Time.deltaTime;

        //if (m_effectTimer <= 0)
        //{
        //    DisableAllEffects();
        //}

        HandleRooted();
        
    }

    // WARNING: THIS IS NOT CURRENTLY SERVER AUTHORITATIVE
    void HandleRooted()
    {
        if (m_characterStatus.IsRooted() && !m_isRootedStart)
        {
            m_isRootedStart = true;
            GetComponent<PlayerPrediction>().MovementMultiplier = 0;
            Rooted.SetActive(true);
        }
        if (!m_characterStatus.IsRooted() && m_isRootedStart)
        {
            m_isRootedStart = false;
            GetComponent<PlayerPrediction>().MovementMultiplier = 1;
            Rooted.SetActive(false);
        }
    }

    //void SetPlayerMovementEnabled(bool isEnabled)
    //{
    //    GetComponent<PlayerPrediction>().MovementMultiplier =
    //        isEnabled ? 1 : 0;

    //    Rooted.SetActive(!isEnabled);
    //}

    //[Rpc(SendTo.ClientsAndHost)]
    //void SetMovementEnabledClientRpc(bool isEnabled)
    //{
    //    GetComponent<PlayerPrediction>().MovementMultiplier =
    //        isEnabled ? 1 : 0;

    //    Rooted.SetActive(!isEnabled);
    //}

    //[Rpc(SendTo.ClientsAndHost)]
    //void SetPlayerInputEnabledClientRpc(bool isEnabled)
    //{
    //    GetComponent<PlayerPrediction>().IsInputDisabled = !isEnabled;
    //}

    private void DisableAllEffects()
    {
        if (Rooted != null) Rooted.SetActive(false);
        if (Stunned != null) Stunned.SetActive(false);
        if (Blind != null) Blind.SetActive(false);
    }
}
