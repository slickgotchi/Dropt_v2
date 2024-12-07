using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ConnectionLostCanvas : MonoBehaviour
{
    public static ConnectionLostCanvas Instance { get; private set; }

    [SerializeField] private GameObject m_container;
    [SerializeField] private TMPro.TextMeshProUGUI m_timerText;

    private PlayerPing m_localPlayerPing;

    private Unity.Netcode.Transports.UTP.UnityTransport m_unityTransport;

    private bool m_isConnectionLost = false;
    private float m_disconnectTimer = 30f;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // grab unity transport if needed
        if (NetworkManager.Singleton != null && m_unityTransport == null)
        {
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            if (transport != null) m_unityTransport = transport;
        }

        m_container.SetActive(false);
    }

    public void SetVisible(bool visible)
    {
        m_container.SetActive(visible);
    }

    private void Update()
    {
        if (Bootstrap.IsServer() || Bootstrap.IsHost()) return;
        if (m_unityTransport == null) return;

        // get local player if not yet got it
        if (m_localPlayerPing == null)
        {
            var players = FindObjectsByType<PlayerPing>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.GetComponent<NetworkObject>().IsLocalPlayer)
                {
                    m_localPlayerPing = player;
                }
            }
        }

        if (m_localPlayerPing == null) return;

        // check time since last ping greater than 5 seconds
        if (m_localPlayerPing.elapsedTimeSinceLastPing > 5 && !m_isConnectionLost)
        {
            m_isConnectionLost = true;
            m_disconnectTimer = (m_unityTransport.DisconnectTimeoutMS/1000) - 5;
            m_container.SetActive(true);
        }

        if (m_localPlayerPing.elapsedTimeSinceLastPing < 5 && m_isConnectionLost)
        {
            m_isConnectionLost = false;
            m_container.SetActive(false);
            m_disconnectTimer = (m_unityTransport.DisconnectTimeoutMS/1000) - 5;
        }

        if (m_isConnectionLost)
        {
            m_disconnectTimer -= Time.deltaTime;
            m_timerText.text = $"{Mathf.CeilToInt(m_disconnectTimer)}s";
        }
    }
}
