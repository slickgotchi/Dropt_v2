using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerDungeonData : NetworkBehaviour
{
    // Public properties with private setters
    public int GltrCount { get; private set; } = 0;
    public int cGHSTCount { get; private set; } = 0;
    public float Essence { get; private set; } = 300;

    private void Update()
    {
        Essence -= Time.deltaTime;
    }

    // Method to add value to GltrCount
    public void AddGltr(int value)
    {
        if (!IsServer) return;

        GltrCount += value;
    }

    // Method to add value to CGHSTCount
    public void AddCGHST(int value)
    {
        if (!IsServer) return;

        cGHSTCount += value;
    }

    public void AddEssence(float value)
    {
        if (!IsServer) return;

        Essence += value;
    }

    // Method to reset counts
    public void Reset()
    {
        if (!IsServer) return;

        GltrCount = 0;
        cGHSTCount = 0;
    }
}
