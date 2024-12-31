using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Mathematics;

public class CustomNetworkTransform : NetworkBehaviour
{
    [SerializeField]
    private float m_syncOnPositionDelta = 0.1f; // Threshold for sending position updates

    public int interpolationDelayTicks = 4; // Delay in seconds for interpolation

    private List<PositionSnapshot> m_positionSnapshots = new List<PositionSnapshot>();

    public void Tick()
    {
        if (IsServer)
        {
            UpdateClientPositionClientRpc(transform.position, NetworkTimer_v2.Instance.TickCurrent);
        }
    }

    private void LateUpdate()
    {
        if (IsClient && !IsHost)
        {
            ClientUpdate();
        }
    }


    private void ClientUpdate()
    {
        if (m_positionSnapshots.Count < 2)
        {
            return;
        }

        if (NetworkTimer_v2.Instance == null)
        {
            Debug.Log("Dont have a network timer");
            return;
        }

        if (ClientServerTickDelta.Instance == null)
        {
            Debug.Log("Dont have a client server tick delta");
            return;
        }

        var targetStartTick = NetworkTimer_v2.Instance.TickCurrent - ClientServerTickDelta.Instance.TickDelta - interpolationDelayTicks;

        PositionSnapshot a;
        PositionSnapshot b;

        for (int i = 0; i < m_positionSnapshots.Count-1; i++)
        {
            if (m_positionSnapshots[i].tick >= targetStartTick)
            {
                a = m_positionSnapshots[i];
                b = m_positionSnapshots[i + 1];

                var newPosition = math.lerp(a.position, b.position, NetworkTimer_v2.Instance.TickFraction);
                transform.position = new Vector3(newPosition.x, newPosition.y, 0);

                return;
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateClientPositionClientRpc(Vector2 newPosition, int serverTick)
    {
        m_positionSnapshots.Add(new PositionSnapshot
        {
            position = newPosition,
            tick = serverTick
        });

        // Remove old snapshots
        while (m_positionSnapshots.Count > 30)
        {
            m_positionSnapshots.RemoveAt(0);
        }

        // order the list
        m_positionSnapshots.Sort((a, b) => a.tick.CompareTo(b.tick));
    }

    private struct PositionSnapshot
    {
        public Vector2 position;
        public int tick;
    }
}
