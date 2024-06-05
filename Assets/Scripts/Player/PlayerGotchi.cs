using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class PlayerGotchi : MonoBehaviour
{
    [SerializeField] Sprite _front;
    [SerializeField] Sprite _back;
    [SerializeField] Sprite _left;
    [SerializeField] Sprite _right;

    [SerializeField] private SpriteRenderer BodySprite;
    [SerializeField] private ParticleSystem DustParticleSystem;

    private Animator animator;
    private PlayerMovementFixed playerMovement;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovementFixed>();
    }

    private void Update()
    {
        UpdateFacingFromMovement();
        UpdateDustParticles();
        UpdateSpriteLean();
        UpdateGotchiAnim();
    }

    private bool IsMoving()
    {
        var velocity = playerMovement.GetVelocity();
        return (math.abs(velocity.x) > 0.5f || math.abs(velocity.y) > 0.5f);
    }

    void UpdateDustParticles()
    {
        if (IsMoving())
        {
            if (!DustParticleSystem.isPlaying) DustParticleSystem.Play();
        }
        else
        {
            if (DustParticleSystem.isPlaying) DustParticleSystem.Stop();
        }
    }

    void UpdateFacingFromMovement()
    {
        if (!IsMoving()) return;
        Vector2 velocity = playerMovement.GetVelocity();

        if (velocity.y > math.abs(velocity.x)) BodySprite.sprite = _back;
        if (velocity.y < -math.abs(velocity.x)) BodySprite.sprite = _front;
        if (velocity.x <= -math.abs(velocity.y)) BodySprite.sprite = _left;
        if (velocity.x >= math.abs(velocity.y)) BodySprite.sprite = _right;
    }

    private void UpdateSpriteLean()
    {
        float rotation = 0;

        if (IsMoving())
        {
            Vector2 direction = playerMovement.GetDirection();

            float vx = direction.x;
            float vy = direction.y;

            float ROT = 4;


            if (vy < -(math.abs(vx) + 0.05))
            {
                // up
                rotation = 0;
            }
            else if (vy > (math.abs(vx) + 0.05))
            {
                // down
                rotation = 0;
            }
            else if (vx < -(math.abs(vy) - 0.05))
            {
                // left
                float newAngle = vy < -0.1 ? 3*ROT : vy > 0.1 ? -ROT : ROT;
                rotation = newAngle;
            }
            else if (vx > (math.abs(vy) - 0.05))
            {
                // right
                float newAngle = vy < -0.1 ? -3*ROT : vy > 0.1 ? ROT : -ROT;
                rotation = newAngle;
            }
        }
        else
        {
            rotation = 0;
        }

        BodySprite.transform.rotation = Quaternion.Euler(new Vector3(0,0,rotation));
    }

    void UpdateGotchiAnim()
    {
        if (IsMoving())
        {
            animator.Play("Player_Move");
        }
        else
        {
            animator.Play("Player_Idle");
        }
    }
}
