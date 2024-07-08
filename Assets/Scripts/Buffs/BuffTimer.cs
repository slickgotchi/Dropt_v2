using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffTimer : MonoBehaviour
{
    private float m_duration = 10f;
    private NetworkCharacter m_networkCharacter;
    private BuffObject m_buff;
    private float m_timer = 0f;

    // Start is called before the first frame update
    public void StartBuff(BuffObject buffObject, NetworkCharacter networkCharacter, float duration)
    {
        m_timer = 0;
        m_duration = duration;
        m_buff = buffObject;
        m_networkCharacter = networkCharacter;

        networkCharacter.AddBuffObject(buffObject);

        Debug.Log("Buff started for " + duration + " seconds");
    }

    public void EndBuff()
    {
        m_networkCharacter.RemoveBuffObject(m_buff);
    }

    // Update is called once per frame
    void Update()
    {
        m_timer += Time.deltaTime;

        if (m_timer >= m_duration)
        {
            EndBuff();
            Destroy(gameObject);
        }
    }
}
