using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLibrary : MonoBehaviour
{
    public static AudioLibrary Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep AudioManager persistent across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public AudioClip TitleScreen;
    public AudioClip ApeVillage;
    public AudioClip UndergroundForest;
    public AudioClip RestFloor;
}
