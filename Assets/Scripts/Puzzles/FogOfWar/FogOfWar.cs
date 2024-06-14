using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FogOfWar : NetworkBehaviour
{
    public float FadeOutDuration = 1f;
    public List<Tilemap> ConnectedTilemaps = new List<Tilemap>();
    private List<float> connectedTilemapsStartAlpha = new List<float>();

    private NetworkVariable<bool> m_isCleared;
    private bool m_isFadeOutStarted = false;
    private bool m_isFadeOutFinished = false;
    private float m_startAlpha = 1f;
    private float m_fadeOutTimer = 1f;
    private Tilemap m_tilemap;

    private void Awake()
    {
        m_isCleared = new NetworkVariable<bool>(false);
        m_tilemap = GetComponent<Tilemap>();

        for (int i = 0; i < ConnectedTilemaps.Count; i++)
        {
            connectedTilemapsStartAlpha.Add(ConnectedTilemaps[i].color.a);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        var player = collision.gameObject.GetComponent<PlayerController>();
        if (player == null) return;

        m_isCleared.Value = true;
    }

    private void Update()
    {
        // return if we've already faded out this fog of war
        if (m_isFadeOutFinished) return;

        // start fade out if we are cleared but fade out not started
        if (m_isCleared.Value && !m_isFadeOutStarted)
        {
            m_isFadeOutStarted = true;
            m_startAlpha = m_tilemap.color.a;
            m_fadeOutTimer = FadeOutDuration;

            for (int i = 0; i < ConnectedTilemaps.Count; i++)
            {
                connectedTilemapsStartAlpha[i] = ConnectedTilemaps[i].color.a;
            }
        }

        // handle fading out
        if (m_isFadeOutStarted)
        {
            m_fadeOutTimer -= Time.deltaTime;
            float newAlpha = m_fadeOutTimer > 0 ? m_fadeOutTimer / FadeOutDuration * m_startAlpha : 0;
            Color color = m_tilemap.color;
            color.a = newAlpha;
            m_tilemap.color = color;

            for (int i = 0; i < ConnectedTilemaps.Count; i++)
            {
                newAlpha = m_fadeOutTimer > 0 ? m_fadeOutTimer / FadeOutDuration * connectedTilemapsStartAlpha[i] : 0;
                color = ConnectedTilemaps[i].color;
                color.a = newAlpha;
                ConnectedTilemaps[i].color = color;
            }
        }
    }
}
