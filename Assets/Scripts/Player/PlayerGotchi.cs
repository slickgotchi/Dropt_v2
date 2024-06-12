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
    private PlayerMovementAndDash playerMovement;

    private Vector3 m_velocity;
    private float m_rotation;
    private Vector3 m_facingDirection;
    private bool m_isMoving;
    private bool m_isDropSpawning;

    private LocalVelocity m_localVelocity;



    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovementAndDash>();
        m_localVelocity = GetComponent<LocalVelocity>();
    }

    private void Update()
    {
        if (IsServer && !IsHost) return;

        if (IsLocalPlayer)
        {
            m_velocity = playerMovement.GetVelocity();
            m_facingDirection = playerMovement.GetFacingDirection();
            m_isMoving = math.abs(m_velocity.x) > 0.1f || math.abs(m_velocity.y) > 0.1f;
        } else
        {
            m_velocity = m_localVelocity.Value;
            m_facingDirection = m_localVelocity.LastNonZeroVelocity.normalized;
            m_isMoving = m_localVelocity.IsMoving;
        }

        m_rotation = CalculateSpriteLean();

        UpdateGotchiAnim();

        UpdateFacingFromMovement();
        UpdateDustParticles();
        BodySprite.transform.rotation = Quaternion.Euler(new Vector3(0, 0, m_rotation));
    }

    public void DropSpawn()
    {
        if (!IsServer) return;

        m_isDropSpawning = true;
        animator.Play("Player_Drop");
    }

    public bool IsDropSpawning()
    {
        return m_isDropSpawning;
    }

    public void AnimEvent_EndDropSpawn()
    {
        m_isDropSpawning = false;

        GetComponent<PlayerCamera>().Shake(1.75f, 0.3f);
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
        if (m_facingDirection.y > math.abs(m_facingDirection.x)) BodySprite.sprite = _back;
        if (m_facingDirection.y < -math.abs(m_facingDirection.x)) BodySprite.sprite = _front;
        if (m_facingDirection.x <= -math.abs(m_facingDirection.y)) BodySprite.sprite = _left;
        if (m_facingDirection.x >= math.abs(m_facingDirection.y)) BodySprite.sprite = _right;
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
        if (m_isDropSpawning) return;

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
