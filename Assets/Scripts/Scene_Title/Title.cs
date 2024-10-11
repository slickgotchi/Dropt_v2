using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Title : MonoBehaviour
{
    public AudioClip titleScreenMusic;

    // Start is called before the first frame update
    void Start()
    {
        AudioManager.Instance.PlayMusic(titleScreenMusic);
    }
}
