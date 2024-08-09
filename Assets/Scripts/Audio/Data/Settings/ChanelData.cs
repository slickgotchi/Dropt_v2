using System;
using UnityEngine;

namespace Audio
{
    [Serializable]
    public struct ChanelData
    {
        [SerializeField] private ChannelEnvironmentSettings _settings;
        [SerializeField] private ChanelType _type;
        [SerializeField] private int _capacity;

        public ChanelType Type => _type;
        public int Capacity => _capacity;
        public ChannelEnvironmentSettings Settings => _settings;
    }
}