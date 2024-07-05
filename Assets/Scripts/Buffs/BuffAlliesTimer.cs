using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffAlliesTimer : MonoBehaviour
{
    private List<NetworkCharacter> m_playerCharacters = new List<NetworkCharacter>();
    private BuffObject m_buff;
    private float m_timer = 10f;

    // Start is called before the first frame update
    public void StartBuff(BuffObject buffObject, float duration)
    {
        m_timer = duration;
        m_buff = buffObject;

        ApplyBuffToAllPlayers();
    }

    private void ApplyBuffToAllPlayers()
    {
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            var character = player.GetComponent<NetworkCharacter>();
            bool isFound = false;
            foreach (var playerCharacter in m_playerCharacters)
            {
                if (playerCharacter == character)
                {
                    isFound = true;
                }
            }
            if (!isFound)
            {
                character.AddBuffObject(m_buff);
                m_playerCharacters.Add(character);
            }
        }
    }

    public void EndBuff()
    {
        foreach (var playerCharacter in m_playerCharacters)
        {
            playerCharacter.RemoveBuffObject(m_buff);
        }
        m_playerCharacters.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        m_timer -= Time.deltaTime;

        if (m_timer <= 0)
        {
            EndBuff();
            Destroy(gameObject);
        } else
        {
            ApplyBuffToAllPlayers();
        }
    }
}
