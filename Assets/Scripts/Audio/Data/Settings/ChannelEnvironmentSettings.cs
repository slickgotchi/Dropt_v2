using System;
using UnityEngine;

namespace Audio
{
    [Serializable]
    public struct ChannelEnvironmentSettings
    {
        [SerializeField] private bool _isLoop;
        [SerializeField, Range(0, 5)] private float _dopplerLevel;
        [SerializeField, Range(0, 1)] private int spatialBlend;
        [SerializeField] private AudioRolloffMode _rolloffMode;
        [SerializeField, Min(0)] private float _minDistance;
        [SerializeField, Min(1)] private float _maxDistance;
        
        public float DopplerLevel => _dopplerLevel;

        public AudioRolloffMode RolloffMode => _rolloffMode;

        public float MinDistance => _minDistance;
        
        public bool IsLoop => _isLoop;

        public float MaxDistance => _maxDistance;

        public int SpatialBlend => spatialBlend;
    }
}