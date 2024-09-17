using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RootedEffect : MonoBehaviour
{
    private NetworkCharacter m_networkCharacter;
    private float m_rootedTimer = 0;
    private BuffObject m_buffObject;

    void StartEffect(float duration, BuffObject buffObject)
    {
        m_networkCharacter = GetComponent<NetworkCharacter>();
        if (m_buffObject != null)
        {
            m_networkCharacter.RemoveBuffObject(m_buffObject);
        }

        m_rootedTimer = duration > 0 ? duration : 0.1f;
        m_buffObject = buffObject;

        m_networkCharacter.AddBuffObject(m_buffObject);

        //// if player, disable input
        //var playerPrediction = GetComponent<PlayerPrediction>();
        //if (playerPrediction != null)
        //{
        //    playerPrediction.IsInputEnabled = false;
        //}
    }

    void FinishEffect()
    {
        m_networkCharacter.RemoveBuffObject(m_buffObject);
        m_buffObject = null;

        //// if player, enable input
        //var playerPrediction = GetComponent<PlayerPrediction>();
        //if (playerPrediction != null)
        //{
        //    playerPrediction.IsInputEnabled = true;
        //}
    }

    public static void ApplyRootedEffect(GameObject target, float duration, BuffObject rootedBuff)
    {
        // add the rooted effect monobehaviour to the target
        RootedEffect rootedEffect = target.GetComponent<RootedEffect>();
        if (rootedEffect == null)
        {
            rootedEffect = target.gameObject.AddComponent<RootedEffect>();
        }

        rootedEffect.StartEffect(duration, rootedBuff);
    }

    private void Update()
    {
        m_rootedTimer -= Time.deltaTime;

        if (m_rootedTimer <= 0 && m_buffObject != null)
        {
            FinishEffect();
        }
    }
}
