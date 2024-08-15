﻿using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class ProjectileSoundPlayer : MonoBehaviour
    {
        [SerializeField]
        private AudioClip[] hitSounds;
        
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        public void PlayHitSound()
        {
            // Debug.Log("PlayHit");
            AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
            audioSource.PlayOneShot(clip);
        }
    }
}