using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmParticles : MonoBehaviour
{
    public GameObject AlarmedParent;

    private ParticleSystem m_particleSystem;

    private void Awake()
    {
        m_particleSystem = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!AlarmedParent.HasComponent<CharacterStatus>()) return;

        if (AlarmedParent.GetComponent<CharacterStatus>().IsAlerting())
        {
            if (!m_particleSystem.isPlaying)
            {
                m_particleSystem.Play();
            }
        }
        else
        {
            if (m_particleSystem.isPlaying)
            {
                m_particleSystem.Stop();
            }
        }


    }
}
