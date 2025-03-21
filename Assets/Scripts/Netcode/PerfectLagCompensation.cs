using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Mathematics;
using System;

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

        NetworkTimer_v2.Instance.OnTick += Tick;
    }

    public override void OnNetworkDespawn()
    {
        NetworkTimer_v2.Instance.OnTick -= Tick;
        base.OnNetworkDespawn();
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

            if (m_tickPositions.Count > 40) m_tickPositions.RemoveAt(0);

            m_tickPositions.Sort((a, b) => a.serverTick.CompareTo(b.serverTick));
        }

        if (IsClient)
        {
            var tickDelta = NetworkTimer_v2.Instance.ClientServerTickDelta;
            var currentTick = NetworkTimer_v2.Instance.TickCurrent;
            var targetServerTick = currentTick - tickDelta - NetworkTimer_v2.Instance.InterpolationLagTicks;

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
        InterpolatePosition();
    }

    private void InterpolatePosition() {
        if (!IsClient || IsHost) return;

        var targetServerTick = NetworkTimer_v2.Instance.TickCurrent -
            NetworkTimer_v2.Instance.ClientServerTickDelta - 
            NetworkTimer_v2.Instance.InterpolationLagTicks + 1;
        var fraction = NetworkTimer_v2.Instance.TickFraction;

        for (int i = 1; i < m_tickPositions.Count; i++)
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

    public static void RollbackAllEntities(int targetTick) {
        var enemyControllers = Game.Instance.enemyControllers;
        foreach (var enemyController in enemyControllers) {
            var perfectLagComp = enemyController.GetComponent<PerfectLagCompensation>();
            if (perfectLagComp != null) {
                perfectLagComp.StashPosition();
                perfectLagComp.SetPositionToTick(targetTick);
            }
        }
    }

    public static void UnrollAllEntities() {
        var enemyControllers = Game.Instance.enemyControllers;
        foreach (var enemyController in enemyControllers)
        {
            var perfectLagComp = enemyController.GetComponent<PerfectLagCompensation>();
            if (perfectLagComp != null) {
                perfectLagComp.RestorePositionFromStash();
            }
        }
    }


    public static int GetRollbackTargetTickForPlayer(GameObject player, int clientServerActivationTickDelta) {
        int targetTick = NetworkTimer_v2.Instance.TickCurrent;
        int interpolationLagTicks = NetworkTimer_v2.Instance.InterpolationLagTicks;

        var playerPing = player.GetComponent<PlayerPing>();
        if (playerPing == null) {
            Debug.LogWarning("null playerPing not allowed!!");
            return targetTick;
        }

        if (Bootstrap.IsClient()) {
            int tickDelta = playerPing.Client_ClientLocalServerReceived_TickDelta;

            targetTick = 
                NetworkTimer_v2.Instance.TickCurrent -
                tickDelta - interpolationLagTicks + 1;
        }
        if (Bootstrap.IsServer()) {
            int tickDelta = playerPing.Server_ClientReportingServerReceived_TickDelta;
            
            targetTick = NetworkTimer_v2.Instance.TickCurrent -
                tickDelta - interpolationLagTicks +
                clientServerActivationTickDelta;
        }

        return targetTick;
    }

    private Vector3 m_stashPosition;

    public void StashPosition()
    {
        m_stashPosition = transform.position;
    }

    public void SetPositionToTick(int tick)
    {
        foreach (var tickPosition in m_tickPositions)
        {
            if (tickPosition.serverTick == tick)
            {
                transform.position = tickPosition.position;
                break;
            }
        }
    }

    public void RestorePositionFromStash()
    {
        transform.position = m_stashPosition;
    }


    [Rpc(SendTo.ClientsAndHost)]
    void SendPositionClientRpc(Vector3 position, int serverTick)
    {
        m_visualA.transform.position = position;

        if (IsHost) return;

        m_tickPositions.Add(new TickPosition { serverTick = serverTick, position = position });

        if (m_tickPositions.Count > 40) m_tickPositions.RemoveAt(0);

        // sort tick positions
        m_tickPositions.Sort((a, b) => a.serverTick.CompareTo(b.serverTick));
    }

    struct TickPosition
    {
        public int serverTick;
        public Vector3 position;
    }
}
