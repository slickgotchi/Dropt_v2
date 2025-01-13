using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCountedBuff : MonoBehaviour
{
    private BuffObject m_buffObject;
    private int m_numberLevels = 0;
    private int m_levelCount = 0;
    private NetworkCharacter m_networkCharacter;

    public bool TryAddBuffToPlayer(BuffObject buffObject, NetworkCharacter networkCharacter, int numLevels)
    {
        m_buffObject = buffObject;
        m_numberLevels = numLevels;
        m_networkCharacter = networkCharacter;

        // bail out of this init if character already has this buff
        if (m_networkCharacter.HasBuffObject(buffObject))
        {
            Destroy(this.gameObject);
            return false;
        }
        else
        {
            m_networkCharacter.AddBuffObject(m_buffObject);
            return true;
        }
    }

    public bool IsBuffAlreadyOnPlayer(BuffObject buffObject, NetworkCharacter networkCharacter)
    {
        if (buffObject == null) { Debug.LogWarning("buffObject = null"); return false; }
        if (networkCharacter == null) { Debug.LogWarning("networkCharacter = null"); return false; }

        // see if we already have that buff
        return networkCharacter.HasBuffObject(buffObject);
    }

    public void IncrementLevelCount()
    {
        m_levelCount++;

        if (m_networkCharacter != null && m_levelCount > m_numberLevels && this.gameObject != null && m_buffObject != null)
        {
            m_networkCharacter.RemoveBuffObject(m_buffObject);
            Destroy(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (m_networkCharacter != null && m_buffObject != null)
        {
            m_networkCharacter.RemoveBuffObject(m_buffObject);
        }
    }
}
