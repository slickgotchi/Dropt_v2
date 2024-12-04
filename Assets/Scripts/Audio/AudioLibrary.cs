using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLibrary : MonoBehaviour
{
    public static AudioLibrary Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("Music")]
    public AudioClip TitleScreen;
    public AudioClip ApeVillage;
    public AudioClip UndergroundForest;
    public AudioClip RestFloor;

    [Header("SFX")]
    public AudioClip HitInorganic;
    public AudioClip HitOrganic;
    public AudioClip HitCrafted;
    public AudioClip EnemyHurt;
}
