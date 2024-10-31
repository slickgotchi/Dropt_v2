using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NPCPositionInterpolator : NetworkBehaviour
{
    public float interpolationTime = 0.1f; // Time between server updates (or target interpolation delay)

    private struct State
    {
        public Vector3 position;
        public float timestamp;
    }

    private Queue<State> stateBuffer = new Queue<State>();
    private State targetState; // The latest received position from the server
    private State previousState; // The previous position for interpolation

    private void Start()
    {
        // Initialize with current position
        targetState = new State { position = transform.position, timestamp = Time.time };
        previousState = targetState;
    }

    void Update()
    {
        // This logic runs only on clients to interpolate between received position updates
        if (IsClient && !IsOwner && stateBuffer.Count > 0)
        {
            // Perform interpolation between the previous and target state
            float lerpTime = (Time.time - previousState.timestamp) / (targetState.timestamp - previousState.timestamp);
            transform.position = Vector3.Lerp(previousState.position, targetState.position, lerpTime);

            // If we've reached the target state, pop it off the buffer and continue
            if (lerpTime >= 1f)
            {
                previousState = targetState;
                stateBuffer.Dequeue();

                if (stateBuffer.Count > 0)
                {
                    targetState = stateBuffer.Peek(); // Set the next target state from the buffer
                }
            }
        }
    }

    [ServerRpc]
    public void UpdatePositionServerRpc(Vector3 newPosition)
    {
        // This is called by the server to propagate position updates to all clients
        UpdatePositionClientRpc(newPosition);
    }

    [ClientRpc]
    void UpdatePositionClientRpc(Vector3 newPosition)
    {
        // Add the received position update to the interpolation buffer
        ReceiveStateUpdate(newPosition);
    }

    public void ReceiveStateUpdate(Vector3 newPosition)
    {
        // Called when we receive a new position update from the server
        State newState = new State { position = newPosition, timestamp = Time.time + interpolationTime };

        if (stateBuffer.Count == 0)
        {
            previousState = targetState;
            targetState = newState;
        }

        stateBuffer.Enqueue(newState);
    }

    // Server-side code to send the position updates
    private void FixedUpdate()
    {
        if (IsServer)
        {
            // Server regularly sends updates to all clients
            UpdatePositionServerRpc(transform.position);
        }
    }
}
