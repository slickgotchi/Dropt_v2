using Dropt;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class SpriteFlipFromLocalVelocity : MonoBehaviour
{
    public SpriteRenderer SpriteToFlip;

    private LocalVelocity m_localVelocity;

    private void Awake()
    {
        m_localVelocity = GetComponent<LocalVelocity>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_localVelocity == null || !m_localVelocity.IsMoving) return;

        SpriteToFlip.flipX = m_localVelocity.Value.x > 0 ? false : true;
    }
}
