using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    public SpriteRenderer SpriteToFlip;

    private LocalVelocity m_localVelocity;

    public enum Facing { Left, Right }
    public Facing FacingDirection;

    private float m_facingTimer = 0f;

    private void Awake()
    {
        m_localVelocity = GetComponent<LocalVelocity>();
    }

    // Update is called once per frame
    void Update()
    {
        if (SpriteToFlip == null) return;

        m_facingTimer -= Time.deltaTime;

        if (m_facingTimer > 0f)
        {
            SpriteToFlip.flipX = FacingDirection == Facing.Left;
        } 
        else if (m_localVelocity.IsMoving)
        {
            SpriteToFlip.flipX = m_localVelocity.Value.x > 0 ? false : true;
        }
    }

    public void SetFacingDirection(Facing facingDirection, float facingTimer = 0.5f)
    {
        m_facingTimer = facingTimer;
        FacingDirection = facingDirection;
    }
}
