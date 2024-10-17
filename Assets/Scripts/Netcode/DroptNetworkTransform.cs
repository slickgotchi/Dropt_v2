using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Dropt;
using Unity.Mathematics;

// NOTE: THIS SCRIPT NOT REQUIRED
// a large part of NetworkTransform leading to janky behaviour was
// due to not setting "unreliable packets" to true in the NetworkTransform.
// this resulted in lost packets being buffered and then released all at
// once when they were found again.

public class DroptNetworkTransform : NetworkBehaviour
{
    public int interpolationDelayTicks = 2;

    struct PositionState
    {
        public Vector3 position;
        public int tick;
    }

    private List<PositionState> m_positionStateBuffer;
    private int k_bufferSize = 32;

    private int m_clientToServerTickDelta = 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_positionStateBuffer = new List<PositionState>();
    }

    private void Update()
    {
        if (IsClient)
        {
            // Calculate the target ticks for interpolation
            var startTick = NetworkTimer_v2.Instance.TickCurrent -
                m_clientToServerTickDelta - interpolationDelayTicks;
            var finishTick = startTick + 1;

            // Find the start and finish states in the buffer
            PositionState? startState = FindPositionForTick(startTick);
            PositionState? finishState = FindPositionForTick(finishTick);

            // If both states exist, interpolate between them
            if (startState.HasValue && finishState.HasValue)
            {
                var alpha = NetworkTimer_v2.Instance.TickFraction;
                transform.position = math.lerp(startState.Value.position, finishState.Value.position, alpha);
                //Debug.Log("startTick: " + startState.Value.tick + ", finishTick: " + finishState.Value.tick + ", pos: " + transform.position);
            } else
            {
                //Debug.Log("No value exists!");
                //m_isSetNew = true;
            }
        }
    }

    public void Tick()
    {
        if (IsServer)
        {
            SetPositionClientRpc(transform.position, NetworkTimer_v2.Instance.TickCurrent);
        }
    }

    float lastTime = 0;

    int lastTick = 0;

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Unreliable)]
    //[Rpc(SendTo.ClientsAndHost)]
    void SetPositionClientRpc(Vector3 pos, int serverTick)
    {
        var currTime = Time.time;
        var delta = currTime - lastTime;
        lastTime = currTime;

        m_positionStateBuffer.Add(new PositionState
        {
            position = pos,
            tick = serverTick,
        });
        //Debug.Log("added new position: " + pos + ", at tick: " + serverTick + ", delta: " + delta);

        if (math.abs(serverTick-lastTick) > 1)
        {
            Debug.Log("Missed a tick");
        }
        lastTick = serverTick;

        if (m_positionStateBuffer.Count > k_bufferSize) m_positionStateBuffer.RemoveAt(0);

        SortPositionBufferByTick();
        FillTickGaps();

        // save current tick delta
        AddToTickDelta(NetworkTimer_v2.Instance.TickCurrent +
            NetworkTimer_v2.Instance.TickFraction - serverTick);
    }

    void SortPositionBufferByTick()
    {
        m_positionStateBuffer.Sort((a, b) => a.tick.CompareTo(b.tick));
    }

    private List<float> m_tickDeltas = new List<float>();
    bool m_isSetNew = true;

    void AddToTickDelta(float tickDelta)
    {
        m_tickDeltas.Add(tickDelta);

        if (m_tickDeltas.Count > 50) m_tickDeltas.RemoveAt(0);

        float sum = 0;
        foreach (var td in m_tickDeltas) sum += td;

        var meanTickDelta = (int)math.round(sum / m_tickDeltas.Count);

        if (math.abs(m_clientToServerTickDelta - meanTickDelta) > 5 || m_isSetNew)
        {
            m_clientToServerTickDelta = meanTickDelta;
            m_isSetNew = false;
            Debug.Log("set new tick delta: " + meanTickDelta);
        }
    }


    // Finds the position for a given tick
    private PositionState? FindPositionForTick(int tick)
    {
        for (int i = 0; i < m_positionStateBuffer.Count; i++)
        {
            if (m_positionStateBuffer[i].tick == tick)
            {
                return m_positionStateBuffer[i];
            }
        }

        return null; // Return null if no valid position state is found for the tick
    }


    void FillTickGaps()
    {
        // Iterate through the buffer and check for gaps between ticks
        for (int i = 0; i < m_positionStateBuffer.Count - 1; i++)
        {
            var currentState = m_positionStateBuffer[i];
            var nextState = m_positionStateBuffer[i + 1];

            // Check if there is a gap between current tick and next tick
            if (nextState.tick > currentState.tick + 1)
            {
                // There is a gap, so fill it
                int gapStartTick = currentState.tick + 1;
                int gapEndTick = nextState.tick - 1;

                for (int missingTick = gapStartTick; missingTick <= gapEndTick; missingTick++)
                {
                    // Calculate the interpolation factor (alpha) for the missing tick
                    float alpha = (float)(missingTick - currentState.tick) / (nextState.tick - currentState.tick);

                    // Interpolate the position between currentState and nextState
                    Vector3 interpolatedPosition = Vector3.Lerp(currentState.position, nextState.position, alpha);

                    // Create the new interpolated PositionState
                    var interpolatedState = new PositionState
                    {
                        position = interpolatedPosition,
                        tick = missingTick
                    };

                    // Insert the interpolated state into the buffer at the correct position
                    m_positionStateBuffer.Insert(i + 1, interpolatedState);

                    // Move the index to account for the newly inserted state
                    i++;
                }
            }
        }
    }

}

