using System.Collections;
using System.Collections.Generic;
using PickupItems;
using PickupItems.Orb;
using Unity.Netcode;
using UnityEngine;

public class PickupItemManager : NetworkBehaviour
{
    public static PickupItemManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public enum Size { Tiny, Small, Medium, Large }

    public GameObject GltrOrbPrefab;
    public GameObject CGHSTOrbPrefab;

    public void SpawnGltr(int value, Vector3 position)
    {
        if (!IsServer) return;

        while (value > 0)
        {
            if (value >= 100)
            {
                GenerateGltrOrb(Size.Large, position);
                value -= 100;
            }
            else if (value >= 25)
            {
                GenerateGltrOrb(Size.Medium, position);
                value -= 25;
            }
            else if (value >= 5)
            {
                GenerateGltrOrb(Size.Small, position);
                value -= 5;
            }
            else
            {
                GenerateGltrOrb(Size.Tiny, position);
                value -= 1;
            }
        }
    }

    public void SpawnBigCGHST(Vector3 position)
    {
        if (!IsServer) return;

        GenerateCGHSTOrb(Size.Large, position);
    }

    public void SpawnSmallCGHST(Vector3 position)
    {
        if (!IsServer) return;

        GenerateCGHSTOrb(Size.Small, position);
    }

    private void GenerateGltrOrb(Size size, Vector3 position, float rand = 0.3f)
    {
        // vary position from that specified slightly
        var deltaX = UnityEngine.Random.Range(-rand, rand);
        var deltaY = UnityEngine.Random.Range(-rand, rand);
        var randPosition = position + new Vector3(deltaX, deltaY, 0);

        var gltrOrb = Instantiate(GltrOrbPrefab);
        gltrOrb.GetComponent<GltrOrb>().Init(size);
        gltrOrb.transform.position = randPosition;
        gltrOrb.GetComponent<NetworkObject>().Spawn();
    }

    private void GenerateCGHSTOrb(Size size, Vector3 position, float rand = 0.5f)
    {
        var deltaX = UnityEngine.Random.Range(-rand, rand);
        var deltaY = UnityEngine.Random.Range(-rand, rand);
        var randPosition = position + new Vector3(deltaX, deltaY, 0);

        var cghstOrb = Instantiate(CGHSTOrbPrefab);
        cghstOrb.GetComponent<CGHSTOrb>().Init(size);
        cghstOrb.transform.position = randPosition;
        cghstOrb.GetComponent<NetworkObject>().Spawn();
    }
}
