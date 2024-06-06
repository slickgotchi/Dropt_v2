using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class PlayerGotchi : NetworkBehaviour
{
    [SerializeField] Sprite _front;
    [SerializeField] Sprite _back;
    [SerializeField] Sprite _left;
    [SerializeField] Sprite _right;

    [SerializeField] private SpriteRenderer BodySprite;
    [SerializeField] private ParticleSystem DustParticleSystem;

    private Animator animator;
    private PlayerMovement playerMovement;

    private Vector3 m_velocity;
    private float m_rotation;
    private Vector3 m_direction;
    private bool m_isMoving;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        // update velocity
        if (IsLocalPlayer)
        {
            m_velocity = playerMovement.GetVelocity();
            m_direction = playerMovement.GetDirection();
            m_isMoving = (math.abs(m_velocity.x) > 0.5f || math.abs(m_velocity.y) > 0.5f);
            m_rotation = CalculateSpriteLean();

            SendDataToServerRpc(m_velocity, m_rotation, m_direction, m_isMoving);

            UpdateGotchiAnim();

            UpdateFacingFromMovement();
            UpdateDustParticles();
            BodySprite.transform.rotation = Quaternion.Euler(new Vector3(0, 0, m_rotation));
        }

    }

    [ServerRpc]
    void SendDataToServerRpc(Vector3 velocity, float rotation, Vector3 direction, bool isMoving)
    {
        SendDataToClientRpc(velocity, rotation, direction, isMoving);
    }

    [ClientRpc]
    void SendDataToClientRpc(Vector3 velocity, float rotation, Vector3 direction, bool isMoving)
    {
        if (IsLocalPlayer) return;

        m_velocity = velocity;
        m_direction = direction;
        m_isMoving = isMoving;
        m_rotation = rotation;

        UpdateFacingFromMovement();
        UpdateDustParticles();
        BodySprite.transform.rotation = Quaternion.Euler(new Vector3(0, 0, m_rotation));
    }

    void UpdateDustParticles()
    {
        if (m_isMoving)
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
        if (!m_isMoving) return;

        if (m_velocity.y > math.abs(m_velocity.x)) BodySprite.sprite = _back;
        if (m_velocity.y < -math.abs(m_velocity.x)) BodySprite.sprite = _front;
        if (m_velocity.x <= -math.abs(m_velocity.y)) BodySprite.sprite = _left;
        if (m_velocity.x >= math.abs(m_velocity.y)) BodySprite.sprite = _right;
    }

    private float CalculateSpriteLean()
    {
        float rotation = 0;

        if (m_isMoving)
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

        return rotation;

        //BodySprite.transform.rotation = Quaternion.Euler(new Vector3(0,0,rotation));
    }

    void UpdateGotchiAnim()
    {
        if (m_isMoving)
        {
            //if (!IsAnimationPlaying("Player_Move"))
            {
                animator.Play("Player_Move");
            }
        }
        else
        {
            //if (!IsAnimationPlaying("Player_Idle")) 
                animator.Play("Player_Idle");
        }
    }

    bool IsAnimationPlaying(string name)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(name);
    }
}
