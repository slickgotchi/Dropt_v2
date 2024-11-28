using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

// stores a position every tick
public class PositionBuffer : MonoBehaviour
{
    struct PositionState
    {
        public Vector3 position;
        public float tickAndFraction;
    }

    private int k_bufferSize = 64;

    private List<PositionState> m_positionStates = new List<PositionState>();

    private Vector3 m_stashPosition;

    private void Update()
    {
        m_positionStates.Add(new PositionState
        {
            position = transform.position,
            tickAndFraction = NetworkTimer_v2.Instance.TickCurrent + NetworkTimer_v2.Instance.TickFraction
        });

        if (m_positionStates.Count > k_bufferSize)
        {
            m_positionStates.RemoveAt(0);
        }
    }

    public Vector3 GetPositionAtTickAndFraction(float targetTickAndFraction)
    {
        // If there are no states, return the current position as a fallback
        if (m_positionStates.Count == 0)
        {
            Debug.LogWarning("No position states available.");
            return transform.position;
        }

        // Set up a variable to track the closest PositionState
        PositionState closestState = m_positionStates[0];
        float closestDifference = math.abs(m_positionStates[0].tickAndFraction - targetTickAndFraction);

        // Loop through the position states to find the closest tick and fraction
        foreach (var state in m_positionStates)
        {
            float difference = math.abs(state.tickAndFraction - targetTickAndFraction);

            if (difference < closestDifference)
            {
                closestState = state;
                closestDifference = difference;
            }
        }

        // Return the position of the closest matching state
        return closestState.position;
    }

    public void StashCurrentPosition()
    {
        m_stashPosition = transform.position;
    }

    public Vector3 GetStashPosition()
    {
        return m_stashPosition;
    }
}
