using Cinemachine;
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
    private PlayerPrediction m_playerMovement;

    private Vector3 m_facingDirection;
    private bool m_isMoving;

    public bool IsDropSpawning { get; private set; }

    private LocalVelocity m_localVelocity;

    private Camera m_camera;
    private CinemachineVirtualCamera m_virtualCamera;
    private Vector3 m_spawnPoint;
    private Vector3 m_preSpawnPoint;

    // spin variables
    public enum SpinDirection { AntiClockwise, Clockwise }
    private SpinDirection m_spinDirection;
    private int m_spinNumber;
    private float m_spinPeriod;
    private float m_spinAngle;
    private float m_spinTimer;

    public enum Facing { Front, Back, Left, Right }
    private Facing m_facing;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        m_playerMovement = GetComponent<PlayerPrediction>();
        m_localVelocity = GetComponent<LocalVelocity>();

        m_camera = GetComponentInChildren<Camera>();
        m_virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
    }

    private void Update()
    {
        if (IsServer && !IsHost) return;

        //if (IsLocalPlayer)
        //{
            m_facingDirection = m_playerMovement.GetFacingDirection();
            m_isMoving = m_playerMovement.IsMoving;
        //} else
        //{
        //    m_facingDirection = m_localVelocity.LastNonZeroVelocity.normalized;
        //    m_isMoving = m_localVelocity.IsMoving;
        //}

        UpdateGotchiAnim();
        UpdateFacingFromMovement();
        UpdateFacingFromSpinning();
        UpdateDustParticles();
        BodySprite.transform.rotation = Quaternion.Euler(new Vector3(0, 0, CalculateSpriteLean()));
    }

    public void DropSpawn(Vector3 currentPosition, Vector3 newSpawnPoint)
    {
        if (!IsServer) return;

        PlayDropAnimationClientRpc(currentPosition, newSpawnPoint);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void PlayDropAnimationClientRpc(Vector3 currentPosition, Vector3 spawnPoint)
    {
        if (!IsLocalPlayer) return;

        m_virtualCamera.Follow = null;

        IsDropSpawning = true;
        animator.Play("Player_Drop");
        m_spawnPoint = spawnPoint;
        m_preSpawnPoint = currentPosition;
    }

    public void AnimEvent_EndDropSpawn()
    {
        IsDropSpawning = false;

        GetComponent<PlayerCamera>().Shake(1.75f, 0.3f);

        // make camera follow player and warp it to our new spawn point
        m_virtualCamera.Follow = transform;
        m_virtualCamera.OnTargetObjectWarped(transform, m_spawnPoint-m_preSpawnPoint);
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

    public void PlayFacingSpin(int spinNumber, float spinPeriod, SpinDirection spinDirection, float startAngle)
    {
        m_spinNumber = spinNumber;
        m_spinPeriod = spinPeriod;
        m_spinDirection = spinDirection;
        m_spinAngle = startAngle;
        m_spinTimer = spinNumber * spinPeriod;
    }

    void UpdateFacingFromSpinning()
    {
        if (m_spinTimer <= 0) return;

        m_spinTimer -= Time.deltaTime;
        float angleDelta = 360 / m_spinPeriod * Time.deltaTime;
        angleDelta *= m_spinDirection == SpinDirection.AntiClockwise ? 1 : -1;
        m_spinAngle += angleDelta;
        SetBodySpriteFromAngle(m_spinAngle);
    }

    /// <summary>
    /// Returns gotchi Facing given a certaub direction
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public Facing GetFacingFromDirection(Vector3 direction)
    {
        direction.z = 0;
        direction.Normalize();
        if (direction.y > math.abs(direction.x)) return Facing.Back;
        if (direction.y < -math.abs(direction.x)) return Facing.Front;
        if (direction.x <= -math.abs(direction.y)) return Facing.Left;
        if (direction.x >= math.abs(direction.y)) return Facing.Right;
        return Facing.Front;
    }

    public void SetBodySpriteFromFacing(Facing facing)
    {
        if (facing == Facing.Back) BodySprite.sprite = _back;
        if (facing == Facing.Front) BodySprite.sprite = _front;
        if (facing == Facing.Left) BodySprite.sprite = _left;
        if (facing == Facing.Right) BodySprite.sprite = _right;
    }

    public void SetBodySpriteFromDirection(Vector3 direction)
    {
        SetBodySpriteFromFacing(GetFacingFromDirection(direction));
    }

    /// <summary>
    /// Returns gotchi Facing given an angle in degrees. Right facing gotchi is 0 degrees
    /// and anticlockwise is positive
    /// </summary>
    /// <param name="angleDegrees"></param>
    /// <returns></returns>
    public Facing GetFacingFromAngle(float angleDegrees)
    {
        // Normalize angleDegrees to be within the range [0, 360)
        angleDegrees = angleDegrees % 360;  // Result will be in the range (-360, 360)
        if (angleDegrees < 0) angleDegrees += 360;

        if (angleDegrees > 45 && angleDegrees <= 135) return Facing.Front;
        if (angleDegrees > 135 && angleDegrees <= 225) return Facing.Left;
        if (angleDegrees > 225 && angleDegrees <= 315) return Facing.Back;
        else return Facing.Right;
    }

    public void SetBodySpriteFromAngle(float angleDegrees)
    {
        SetBodySpriteFromFacing(GetFacingFromAngle(angleDegrees));
    }

    private float CalculateSpriteLean()
    {
        float rotation = 0;

        if (m_isMoving)
        {
            //Vector2 direction = m_playerMovement.GetDirection();
            Vector2 direction = m_facingDirection;

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
    }

    void UpdateGotchiAnim()
    {
        if (IsDropSpawning) return;

        if (m_isMoving)
        {
            animator.Play("Player_Move");
        }
        else
        {
            animator.Play("Player_Idle");
        }
    }
}
