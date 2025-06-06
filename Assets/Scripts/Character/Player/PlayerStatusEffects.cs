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

    private void Awake()
    {
        m_characterStatus = GetComponent<CharacterStatus>();
        Rooted.SetActive(false);
    }


    private void Update()
    {
        HandleRooted();
    }

    // WARNING: THIS IS NOT CURRENTLY SERVER AUTHORITATIVE
    void HandleRooted()
    {
        Rooted.SetActive(m_characterStatus.IsRooted());
        GetComponent<PlayerPrediction>().IsMovementEnabled = !m_characterStatus.IsRooted();
    }

    private void DisableAllEffects()
    {
        if (Rooted != null) Rooted.SetActive(false);
        if (Stunned != null) Stunned.SetActive(false);
        if (Blind != null) Blind.SetActive(false);
    }
}
