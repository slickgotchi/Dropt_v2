using UnityEngine;

namespace Audio
{
    public struct LoadingClip
    {
        public AudioClip Clip { get; }
        public bool Success { get; }


        public LoadingClip(AudioClip clip, bool success)
        {
            Clip = clip;
            Success = success;
        }
    }
}