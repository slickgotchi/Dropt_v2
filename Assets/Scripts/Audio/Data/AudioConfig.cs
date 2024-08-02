using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "Audio Configuration", menuName = "Configs/Audio", order = 0)]
    public class AudioConfig : ScriptableObject
    {
        [SerializeField] private float _fadingDuration;
        [SerializeField, Min(1)] private int _maxNumberOfDuplicates;
        [SerializeField] private ChanelData[] _data;

        public IEnumerable<ChanelData> Data => _data;

        public int MaxNumberOfDuplicates => _maxNumberOfDuplicates;
        public float FadingDuration => _fadingDuration;
    }
}