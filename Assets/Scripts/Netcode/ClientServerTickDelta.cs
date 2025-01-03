using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Mathematics;

public class ClientServerTickDelta : NetworkBehaviour
{
    public static ClientServerTickDelta Instance { get; private set; }

    public float SyncInterval = 1f;

    // add this TickDelta to our current Client NetworkTimer_v2 tick to get
    // the equivalent tick on the server
    [HideInInspector] public int TickDelta = 0;

    private float m_syncTimer = 0f;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (!IsSpawned) return;

        if (IsServer)
        {
            m_syncTimer -= Time.deltaTime;
            if (m_syncTimer < 0f)
            {
                m_syncTimer = SyncInterval;
                SyncTickDeltaClientRpc(NetworkTimer_v2.Instance.TickCurrent);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SyncTickDeltaClientRpc(int serverTick)
    {
        var newTickDelta = NetworkTimer_v2.Instance.TickCurrent - serverTick;
        if (math.abs(newTickDelta - TickDelta) > 5)
        {
            TickDelta = newTickDelta;
            Debug.Log("Updated tick delta");
        }
    }
}
