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
    struct PositionState
    {
        public Vector3 position;
        public int tick;
    }

    private List<PositionState> m_positionStateBuffer;
    private int k_bufferSize = 32;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_positionStateBuffer = new List<PositionState>();
    }

    private void Update()
    {
        if (IsClient)
        {
            var currentTick = NetworkTimer_v2.Instance.TickCurrent;
            var clientServerTickDelta = NetworkTimer_v2.Instance.ClientServerTickDelta;
            var interpolationDelayTicks = NetworkTimer_v2.Instance.DroptNetworkTransformInterpolationDelayTicks;

            if (m_positionStateBuffer.Count < 5) return;

            var targetTick = currentTick - clientServerTickDelta - interpolationDelayTicks;

            int a = -1;
            int b = -1;

            for (int i = 0; i < m_positionStateBuffer.Count - 1; i++)
            {
                if (targetTick == m_positionStateBuffer[i].tick)
                {
                    a = i;
                    b = i + 1;
                    break;
                }
            }

            if (a == -1 || b == -1) return;

            var start = m_positionStateBuffer[a];
            var finish = m_positionStateBuffer[b];
            var fraction = NetworkTimer_v2.Instance.TickFraction;

            transform.position = Vector3.Lerp(start.position, finish.position, fraction);

            /*
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
            */
        }
    }

    public void Tick()
    {
        if (IsServer)
        {
            SetPositionClientRpc(transform.position, NetworkTimer_v2.Instance.TickCurrent);
        }
    }

    [Rpc(SendTo.NotServer)]
    void SetPositionClientRpc(Vector3 pos, int serverTick)
    {
        m_positionStateBuffer.Add(new PositionState
        {
            position = pos,
            tick = serverTick,
        });

        if (m_positionStateBuffer.Count > k_bufferSize) m_positionStateBuffer.RemoveAt(0);

        m_positionStateBuffer.Sort((state1, state2) => state1.tick.CompareTo(state2.tick));

        FillTickGaps();
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

