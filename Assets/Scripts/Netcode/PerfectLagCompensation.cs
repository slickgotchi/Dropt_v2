using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Mathematics;

public class PerfectLagCompensation : NetworkBehaviour
{
    [SerializeField] private GameObject m_visualA;
    [SerializeField] private GameObject m_visualB;

    private List<TickPosition> m_tickPositions = new List<TickPosition>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_visualA.transform.parent = null;
        m_visualB.transform.parent = null;
    }

    public void Tick()
    {
        if (IsServer)
        {
            var position = transform.position;
            var serverTick = NetworkTimer_v2.Instance.TickCurrent;
            SendPositionClientRpc(position, serverTick);

            m_tickPositions.Add(new TickPosition
            {
                position = position,
                serverTick = serverTick
            });

            if (m_tickPositions.Count > 100) m_tickPositions.RemoveAt(0);

            m_tickPositions.Sort((a, b) => a.serverTick.CompareTo(b.serverTick));
        }

        if (IsClient)
        {
            var tickDelta = NetworkTimer_v2.Instance.ClientServerTickDelta;
            var currentTick = NetworkTimer_v2.Instance.TickCurrent;
            var targetServerTick = currentTick - tickDelta - 5;

            foreach (var tickPosition in m_tickPositions)
            {
                if (tickPosition.serverTick == targetServerTick)
                {
                    m_visualB.transform.position = tickPosition.position;
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (IsClient)
        {
            var targetServerTick = NetworkTimer_v2.Instance.TickCurrent -
                NetworkTimer_v2.Instance.ClientServerTickDelta - 5 + 1;
            var fraction = NetworkTimer_v2.Instance.TickFraction;

            for (int i = 0; i < m_tickPositions.Count; i++)
            {
                if (m_tickPositions[i].serverTick == targetServerTick)
                {
                    var posA = m_tickPositions[i - 1].position;
                    var posB = m_tickPositions[i].position;
                    var lerpPos = math.lerp(posA, posB, fraction);
                    transform.position = lerpPos;
                }
            }
        }
    }

    private Vector3 m_stashPosition;

    public void StashPosition()
    {
        m_stashPosition = transform.position;
        //Debug.Log("Stash Position: " + m_stashPosition);
    }

    public void SetPositionToTick(int tick)
    {
        //Debug.Log("Look for targetTick: " + tick);
        foreach (var tickPosition in m_tickPositions)
        {
            if (tickPosition.serverTick == tick)
            {
                //Debug.Log("Found targetTick: " + tickPosition.serverTick +
                //    "and set position to: " + tickPosition.position);
                transform.position = tickPosition.position;
                break;
            }
        }
    }

    public void RestorePositionFromStash()
    {
        transform.position = m_stashPosition;
        //Debug.Log("Restore Stash Position: " + m_stashPosition);
    }


    [Rpc(SendTo.ClientsAndHost)]
    void SendPositionClientRpc(Vector3 position, int serverTick)
    {
        m_visualA.transform.position = position;
        m_tickPositions.Add(new TickPosition { serverTick = serverTick, position = position });

        if (m_tickPositions.Count > 100) m_tickPositions.RemoveAt(0);

        // sort tick positions
        m_tickPositions.Sort((a, b) => a.serverTick.CompareTo(b.serverTick));
    }

    struct TickPosition
    {
        public int serverTick;
        public Vector3 position;
    }
}
