using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerStatusEffects : NetworkBehaviour
{
    public GameObject Rooted;
    public GameObject Stunned;
    public GameObject Blind;

    public enum Effect
    {
        None,
        Rooted,
        Stunned,
        Blind
    }

    private float m_effectTimer = 0f;

    public void SetVisualEffect(Effect effect, float duration)
    {
        if (IsClient || IsHost)
        {
            m_effectTimer = duration;

            switch (effect)
            {
                case Effect.Rooted:
                    DisableAllEffects();
                    Rooted.SetActive(true);
                    break;
                default: break;
            }
        }
        else
        {
            SetVisualEffectClientRpc(effect, duration);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetVisualEffectClientRpc(Effect effect, float duration)
    {
        SetVisualEffect(effect, duration);
    }

    private void Update()
    {
        m_effectTimer -= Time.deltaTime;

        if (m_effectTimer <= 0)
        {
            DisableAllEffects();
        }
    }

    private void DisableAllEffects()
    {
        if (Rooted != null) Rooted.SetActive(false);
        if (Stunned != null) Stunned.SetActive(false);
        if (Blind != null) Blind.SetActive(false);
    }
}
