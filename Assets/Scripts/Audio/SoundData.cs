using UnityEngine;

[System.Serializable]
public class SoundData
{
    public AudioClip AudioClip;
    [Range(0, 1)]
    public float Volume = 0.5f;
}