using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tilemaps
{
    [Serializable]
    public sealed class GeneratorLayer
    {
        [SerializeField] public string Name;
        [SerializeField] public bool Enabled = true;
        [SerializeField] public Tilemap TargetLayer;
        [SerializeField] public TileBase[] Tiles;
        [SerializeField] public Tilemap[] Patterns;
        [SerializeField] public int Height;

        public GeneratorLayer()
        {
            Enabled = true;
        }
    }

    [RequireComponent(typeof(Tilemap))]
    public sealed class TileMapTreeGenerator : MonoBehaviour
    {
        [SerializeField] public GeneratorLayer[] Layers;
        [SerializeField] public int Seed;
    }
}