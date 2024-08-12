using Audio.Game;
using Cinemachine;
using GotchiHub;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class PlayerGotchi : NetworkBehaviour
{
    [Header("Gotchi Sprite Side Views")]
    [SerializeField] Sprite _front;
    [SerializeField] Sprite _back;
    [SerializeField] Sprite _left;
    [SerializeField] Sprite _right;

    [Header("Gotchi GameObject")]
    [SerializeField] GameObject m_gotchi;
    [SerializeField] GameObject m_shadow;

    [Header("Body GameObject and Side Views")]
    [SerializeField] GameObject m_bodyParent;
    [SerializeField] public GameObject BodyFaceFront;
    [SerializeField] public GameObject BodyFaceBack;
    [SerializeField] public GameObject BodyFaceLeft;
    [SerializeField] public GameObject BodyFaceRight;

    [Header("Right Hand GameObject and Side Views")]
    [SerializeField] GameObject m_rightHandParent;
    [SerializeField] GameObject m_rightHandFaceFront;
    [SerializeField] GameObject m_rightHandFaceBack;
    [SerializeField] GameObject m_rightHandFaceLeft;
    [SerializeField] GameObject m_rightHandFaceRight;

    [Header("Left Hand GameObject and Side Views")]
    [SerializeField] GameObject m_leftHandParent;
    [SerializeField] GameObject m_leftHandFaceFront;
    [SerializeField] GameObject m_leftHandFaceBack;
    [SerializeField] GameObject m_leftHandFaceLeft;
    [SerializeField] GameObject m_leftHandFaceRight;

    [Header("Effects")]
    [SerializeField] private ParticleSystem DustParticleSystem;

    private Animator animator;
    private PlayerPrediction m_playerPrediction;

    private Vector3 m_facingDirection;
    private bool m_isMoving;

    public bool IsDropSpawning { get; private set; }

    private LocalVelocity m_localVelocity;

    //private Camera m_camera;
    //private CinemachineVirtualCamera m_virtualCamera;
    private Vector3 m_spawnPoint;
    private Vector3 m_preSpawnPoint;

    // facing spin variables
    public enum SpinDirection { AntiClockwise, Clockwise }
    private SpinDirection m_spinDirection;
    private float m_spinPeriod;
    private float m_spinAngle;
    private float m_spinTimer;

    private float m_leftHandHideTimer = 0;
    private float m_rightHandHideTimer = 0;

    public enum Facing { Front, Back, Left, Right, NA }
    private Facing m_facing;

    private float m_bodyRotation = 0;
    private float m_bodyRotationTimer = 0;

    public GameObject GetGotchi()
    {
        return m_gotchi;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        m_playerPrediction = GetComponent<PlayerPrediction>();
        m_localVelocity = GetComponent<LocalVelocity>();

        //m_camera = GetComponentInChildren<Camera>();
        //m_virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
    }

    private void Update()
    {
        if (IsServer && !IsHost) return;

        m_leftHandHideTimer -= Time.deltaTime;
        m_rightHandHideTimer -= Time.deltaTime;

        m_facingDirection = m_playerPrediction.GetFacingDirection();
        m_isMoving = IsLocalPlayer ? m_playerPrediction.IsMoving : m_localVelocity.IsMoving;

        UpdateGotchiAnim();
        UpdateFacingFromMovement();
        UpdateFacingFromSpinning();
        SetActiveBodyPartsFromFacing(m_facing);
        UpdateDustParticles();

        m_bodyRotationTimer -= Time.deltaTime;
        if (m_bodyRotationTimer > 0)
        {
            // need to disable animator as PlayerGotchi_DropSpawn controls the Gotchi z position
            GetComponent<Animator>().enabled = false;
            m_gotchi.transform.rotation = Quaternion.Euler(new Vector3(0, 0, m_bodyRotation));
        }
        else
        {
            m_gotchi.transform.rotation = Quaternion.identity;
            m_bodyParent.transform.rotation = Quaternion.Euler(new Vector3(0, 0, CalculateSpriteLean()));
            GetComponent<Animator>().enabled = true;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            ResetIdleAnimation();
        }
    }

    private float m_dropSpawnTimer = 0.5f;

    public void DropSpawn(Vector3 currentPosition, Vector3 newSpawnPoint)
    {
        if (!IsServer) return;

        PlayDropAnimationClientRpc(currentPosition, newSpawnPoint);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void PlayDropAnimationClientRpc(Vector3 currentPosition, Vector3 spawnPoint)
    {
        if (!IsLocalPlayer) return;

        //m_virtualCamera.Follow = null;

        IsDropSpawning = true;
        animator.Play("PlayerGotchi_DropSpawn");
        m_spawnPoint = spawnPoint;
        m_preSpawnPoint = currentPosition;

        GameAudioManager.Instance.FallNewLevel(spawnPoint);
    }

    public void ResetIdleAnimation()
    {
        // Local Client
        if (IsLocalPlayer)
        {
            animator.Play("PlayerGotchiBody_Reset");
            animator.Play("PlayerGotchiLeftHand_Reset");
            animator.Play("PlayerGotchiRightHand_Reset");
        }

        // Server
        if (IsServer)
        {
            ResetIdleAnimationClientRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ResetIdleAnimationClientRpc()
    {
        // Remote Client
        if (!IsLocalPlayer)
        {
            animator.Play("PlayerGotchiBody_Reset");
            animator.Play("PlayerGotchiLeftHand_Reset");
            animator.Play("PlayerGotchiRightHand_Reset");
        }
    }

    public void PlayAnimation(string animName)
    {
        // Local Client
        if (IsLocalPlayer)
        {
            animator.Play(animName);
        }

        // Server
        if (IsServer)
        {
            PlayAnimationClientRpc(animName);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayAnimationClientRpc(string animName)
    {
        // Remote Client
        if (!IsLocalPlayer)
        {
            animator.Play(animName);
        }
    }

    public void SetVisible(bool visible)
    {
        // Local Client
        if (IsLocalPlayer)
        {
            m_gotchi.SetActive(visible);
            m_shadow.SetActive(visible);
        }

        // Server
        if (IsServer)
        {
            SetVisibleClientRpc(visible);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetVisibleClientRpc(bool visible)
    {
        // Remote Client
        if (!IsLocalPlayer)
        {
            SetVisible(visible);
        }

    }

    public void AnimEvent_EndDropSpawn()
    {
        IsDropSpawning = false;

        GetComponent<PlayerCamera>().Shake(1.75f, 0.3f);

        //// make camera follow player and warp it to our new spawn point
        //m_virtualCamera.Follow = transform;
        //m_virtualCamera.OnTargetObjectWarped(transform, m_spawnPoint - m_preSpawnPoint);

        // renable collider
        GetComponent<Collider2D>().enabled = true;
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
        if (m_facingDirection.y < -math.abs(m_facingDirection.x)) m_facing = Facing.Front;
        if (m_facingDirection.y > math.abs(m_facingDirection.x)) m_facing = Facing.Back;
        if (m_facingDirection.x <= -math.abs(m_facingDirection.y)) m_facing = Facing.Left;
        if (m_facingDirection.x >= math.abs(m_facingDirection.y)) m_facing = Facing.Right;
    }

    public void SetWeaponSprites(Hand hand, Wearable.NameEnum wearableNameEnum)
    {
        if (wearableNameEnum == Wearable.NameEnum.Unarmed) wearableNameEnum = Wearable.NameEnum.None;

        if (hand == Hand.Left)
        {
            // set sprite
            m_leftHandFaceFront.transform.Find("Wearable").GetComponent<SpriteRenderer>().sprite =
                WeaponSpriteManager.Instance.GetSprite(wearableNameEnum, Facing.Front);
            m_leftHandFaceBack.transform.Find("Wearable").GetComponent<SpriteRenderer>().sprite =
                WeaponSpriteManager.Instance.GetSprite(wearableNameEnum, Facing.Back);
            m_leftHandFaceLeft.transform.Find("Wearable").GetComponent<SpriteRenderer>().sprite =
                WeaponSpriteManager.Instance.GetSprite(wearableNameEnum, Facing.Left);
            m_leftHandFaceRight.transform.Find("Wearable").GetComponent<SpriteRenderer>().sprite =
                WeaponSpriteManager.Instance.GetSprite(wearableNameEnum, Facing.Right);

            // sort order
            var leftHandFaceFrontHandSortingOrder = m_leftHandFaceFront.transform.Find("Hand").
                GetComponent<DynamicSorting>().sortingOrderOffset;
            var leftHandFaceBackHandSortingOrder = m_leftHandFaceBack.transform.Find("Hand").
                GetComponent<DynamicSorting>().sortingOrderOffset;
            var leftHandFaceLeftHandSortingOrder = m_leftHandFaceLeft.transform.Find("Hand").
                GetComponent<DynamicSorting>().sortingOrderOffset;
            var leftHandFaceRightHandSortingOrder = m_leftHandFaceRight.transform.Find("Hand").
                GetComponent<DynamicSorting>().sortingOrderOffset;

            // set sorting order
            m_leftHandFaceFront.transform.Find("Wearable").GetComponent<DynamicSorting>().sortingOrderOffset =
                WeaponSpriteManager.Instance.GetSpriteOrder(wearableNameEnum, Facing.Front) == 1 ?
                leftHandFaceFrontHandSortingOrder - 1 : leftHandFaceFrontHandSortingOrder + 1;
            m_leftHandFaceBack.transform.Find("Wearable").GetComponent<DynamicSorting>().sortingOrderOffset =
                WeaponSpriteManager.Instance.GetSpriteOrder(wearableNameEnum, Facing.Back) == 1 ?
                leftHandFaceBackHandSortingOrder - 1 : leftHandFaceBackHandSortingOrder + 1;
            m_leftHandFaceLeft.transform.Find("Wearable").GetComponent<DynamicSorting>().sortingOrderOffset =
                WeaponSpriteManager.Instance.GetSpriteOrder(wearableNameEnum, Facing.Left) == 1 ?
                leftHandFaceLeftHandSortingOrder - 1 : leftHandFaceLeftHandSortingOrder + 1;
            m_leftHandFaceRight.transform.Find("Wearable").GetComponent<DynamicSorting>().sortingOrderOffset =
                WeaponSpriteManager.Instance.GetSpriteOrder(wearableNameEnum, Facing.Right) == 1 ?
                leftHandFaceRightHandSortingOrder - 1 : leftHandFaceRightHandSortingOrder + 1;

            // offsets
            m_leftHandFaceFront.transform.Find("Wearable").transform.localPosition =
                WeaponSpriteManager.Instance.GetSpriteOffset(wearableNameEnum, Facing.Front);
            m_leftHandFaceBack.transform.Find("Wearable").transform.localPosition =
                WeaponSpriteManager.Instance.GetSpriteOffset(wearableNameEnum, Facing.Back);
            m_leftHandFaceLeft.transform.Find("Wearable").transform.localPosition =
                WeaponSpriteManager.Instance.GetSpriteOffset(wearableNameEnum, Facing.Left);
            m_leftHandFaceRight.transform.Find("Wearable").transform.localPosition =
                WeaponSpriteManager.Instance.GetSpriteOffset(wearableNameEnum, Facing.Right);
        }
        else
        {
            // set sprite
            m_rightHandFaceFront.transform.Find("Wearable").GetComponent<SpriteRenderer>().sprite =
                WeaponSpriteManager.Instance.GetSprite(wearableNameEnum, Facing.Front);
            m_rightHandFaceBack.transform.Find("Wearable").GetComponent<SpriteRenderer>().sprite =
                WeaponSpriteManager.Instance.GetSprite(wearableNameEnum, Facing.Back);
            m_rightHandFaceLeft.transform.Find("Wearable").GetComponent<SpriteRenderer>().sprite =
                WeaponSpriteManager.Instance.GetSprite(wearableNameEnum, Facing.Left);
            m_rightHandFaceRight.transform.Find("Wearable").GetComponent<SpriteRenderer>().sprite =
                WeaponSpriteManager.Instance.GetSprite(wearableNameEnum, Facing.Right);

            var rightHandFaceFrontHandSortingOrder = m_rightHandFaceFront.transform.Find("Hand").
                GetComponent<DynamicSorting>().sortingOrderOffset;
            var rightHandFaceBackHandSortingOrder = m_rightHandFaceBack.transform.Find("Hand").
                GetComponent<DynamicSorting>().sortingOrderOffset;
            var rightHandFaceLeftHandSortingOrder = m_rightHandFaceLeft.transform.Find("Hand").
                GetComponent<DynamicSorting>().sortingOrderOffset;
            var rightHandFaceRightHandSortingOrder = m_rightHandFaceRight.transform.Find("Hand").
                GetComponent<DynamicSorting>().sortingOrderOffset;

            // set sorting order
            m_rightHandFaceFront.transform.Find("Wearable").GetComponent<DynamicSorting>().sortingOrderOffset =
                WeaponSpriteManager.Instance.GetSpriteOrder(wearableNameEnum, Facing.Front) == 1 ?
                rightHandFaceFrontHandSortingOrder - 1 : rightHandFaceFrontHandSortingOrder + 1;
            m_rightHandFaceBack.transform.Find("Wearable").GetComponent<DynamicSorting>().sortingOrderOffset =
                WeaponSpriteManager.Instance.GetSpriteOrder(wearableNameEnum, Facing.Back) == 1 ?
                rightHandFaceBackHandSortingOrder - 1 : rightHandFaceBackHandSortingOrder + 1;
            m_rightHandFaceLeft.transform.Find("Wearable").GetComponent<DynamicSorting>().sortingOrderOffset =
                WeaponSpriteManager.Instance.GetSpriteOrder(wearableNameEnum, Facing.Left) == 1 ?
                rightHandFaceLeftHandSortingOrder - 1 : rightHandFaceLeftHandSortingOrder + 1;
            m_rightHandFaceRight.transform.Find("Wearable").GetComponent<DynamicSorting>().sortingOrderOffset =
                WeaponSpriteManager.Instance.GetSpriteOrder(wearableNameEnum, Facing.Right) == 1 ?
                rightHandFaceRightHandSortingOrder - 1 : rightHandFaceRightHandSortingOrder + 1;

            // offsets
            m_rightHandFaceFront.transform.Find("Wearable").transform.localPosition =
                WeaponSpriteManager.Instance.GetSpriteOffset(wearableNameEnum, Facing.Front);
            m_rightHandFaceBack.transform.Find("Wearable").transform.localPosition =
                WeaponSpriteManager.Instance.GetSpriteOffset(wearableNameEnum, Facing.Back);
            m_rightHandFaceLeft.transform.Find("Wearable").transform.localPosition =
                WeaponSpriteManager.Instance.GetSpriteOffset(wearableNameEnum, Facing.Left);
            m_rightHandFaceRight.transform.Find("Wearable").transform.localPosition =
                WeaponSpriteManager.Instance.GetSpriteOffset(wearableNameEnum, Facing.Right);
        }
    }

    void SetActiveBodyPartsFromFacing(Facing facing)
    {
        if (facing == Facing.Front)
        {
            // body
            BodyFaceFront.SetActive(true);
            BodyFaceBack.SetActive(false);
            BodyFaceLeft.SetActive(false);
            BodyFaceRight.SetActive(false);

            // right hand
            if (m_rightHandHideTimer < 0) m_rightHandFaceFront.SetActive(true);
            m_rightHandFaceBack.SetActive(false);
            m_rightHandFaceLeft.SetActive(false);
            m_rightHandFaceRight.SetActive(false);

            // left hand
            if (m_leftHandHideTimer < 0) m_leftHandFaceFront.SetActive(true);
            m_leftHandFaceBack.SetActive(false);
            m_leftHandFaceLeft.SetActive(false);
            m_leftHandFaceRight.SetActive(false);
        }
        if (facing == Facing.Back)
        {
            // body
            BodyFaceFront.SetActive(false);
            BodyFaceBack.SetActive(true);
            BodyFaceLeft.SetActive(false);
            BodyFaceRight.SetActive(false);

            // right hand
            m_rightHandFaceFront.SetActive(false);
            if (m_rightHandHideTimer < 0) m_rightHandFaceBack.SetActive(true);
            m_rightHandFaceLeft.SetActive(false);
            m_rightHandFaceRight.SetActive(false);

            // left hand
            m_leftHandFaceFront.SetActive(false);
            if (m_leftHandHideTimer < 0) m_leftHandFaceBack.SetActive(true);
            m_leftHandFaceLeft.SetActive(false);
            m_leftHandFaceRight.SetActive(false);
        }
        if (facing == Facing.Left)
        {
            // body
            BodyFaceFront.SetActive(false);
            BodyFaceBack.SetActive(false);
            BodyFaceLeft.SetActive(true);
            BodyFaceRight.SetActive(false);

            // right hand
            m_rightHandFaceFront.SetActive(false);
            m_rightHandFaceBack.SetActive(false);
            if (m_rightHandHideTimer < 0) m_rightHandFaceLeft.SetActive(true);
            m_rightHandFaceRight.SetActive(false);

            // left hand
            m_leftHandFaceFront.SetActive(false);
            m_leftHandFaceBack.SetActive(false);
            if (m_leftHandHideTimer < 0) m_leftHandFaceLeft.SetActive(true);
            m_leftHandFaceRight.SetActive(false);
        }
        if (facing == Facing.Right)
        {
            // body
            BodyFaceFront.SetActive(false);
            BodyFaceBack.SetActive(false);
            BodyFaceLeft.SetActive(false);
            BodyFaceRight.SetActive(true);

            // right hand
            m_rightHandFaceFront.SetActive(false);
            m_rightHandFaceBack.SetActive(false);
            m_rightHandFaceLeft.SetActive(false);
            if (m_rightHandHideTimer < 0) m_rightHandFaceRight.SetActive(true);

            // left hand
            m_leftHandFaceFront.SetActive(false);
            m_leftHandFaceBack.SetActive(false);
            m_leftHandFaceLeft.SetActive(false);
            if (m_leftHandHideTimer < 0) m_leftHandFaceRight.SetActive(true);
        }
    }



    public void HideHand(Hand hand, float duration = 0.5f)
    {
        if (IsLocalPlayer)
        {
            if (hand == Hand.Left)
            {
                // left hand
                m_leftHandHideTimer = duration;
                m_leftHandFaceFront.SetActive(false);
                m_leftHandFaceBack.SetActive(false);
                m_leftHandFaceLeft.SetActive(false);
                m_leftHandFaceRight.SetActive(false);

            }
            if (hand == Hand.Right)
            {
                // right hand
                m_rightHandHideTimer = duration;
                m_rightHandFaceFront.SetActive(false);
                m_rightHandFaceBack.SetActive(false);
                m_rightHandFaceLeft.SetActive(false);
                m_rightHandFaceRight.SetActive(false);
            }
        }

        if (IsServer)
        {
            HideHandClientRpc(hand, duration);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void HideHandClientRpc(Hand hand, float duration = 0.5f)
    {
        if (!IsLocalPlayer)
        {
            if (hand == Hand.Left)
            {
                // left hand
                m_leftHandHideTimer = duration;
                m_leftHandFaceFront.SetActive(false);
                m_leftHandFaceBack.SetActive(false);
                m_leftHandFaceLeft.SetActive(false);
                m_leftHandFaceRight.SetActive(false);

            }
            if (hand == Hand.Right)
            {
                // right hand
                m_rightHandHideTimer = duration;
                m_rightHandFaceFront.SetActive(false);
                m_rightHandFaceBack.SetActive(false);
                m_rightHandFaceLeft.SetActive(false);
                m_rightHandFaceRight.SetActive(false);
            }
        }
    }

    public void SetGotchiRotation(float angleDegrees, float duration = 0.5f)
    {
        if (IsLocalPlayer)
        {
            // Normalize angleDegrees to be within the range [0, 360)
            angleDegrees = angleDegrees % 360;  // Result will be in the range (-360, 360)
            if (angleDegrees < 0) angleDegrees += 360;

            m_bodyRotation = angleDegrees;
            m_bodyRotationTimer = duration;
        }

        if (IsServer)
        {
            SetGotchiRotationClientRpc(angleDegrees, duration);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetGotchiRotationClientRpc(float angleDegrees, float duration)
    {
        if (!IsLocalPlayer)
        {
            // Normalize angleDegrees to be within the range [0, 360)
            angleDegrees = angleDegrees % 360;  // Result will be in the range (-360, 360)
            if (angleDegrees < 0) angleDegrees += 360;

            m_bodyRotation = angleDegrees;
            m_bodyRotationTimer = duration;
        }
    }

    public void PlayFacingSpin(int spinNumber, float spinPeriod, SpinDirection spinDirection, float startAngle)
    {
        if (IsLocalPlayer)
        {
            m_spinPeriod = spinPeriod;
            m_spinDirection = spinDirection;
            m_spinAngle = startAngle;
            m_spinTimer = spinNumber * spinPeriod;
        }

        if (IsServer)
        {
            PlayFacingSpinClientRpc(spinNumber, spinPeriod, spinDirection, startAngle);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayFacingSpinClientRpc(int spinNumber, float spinPeriod, SpinDirection spinDirection, float startAngle)
    {
        if (!IsLocalPlayer)
        {
            m_spinPeriod = spinPeriod;
            m_spinDirection = spinDirection;
            m_spinAngle = startAngle;
            m_spinTimer = spinNumber * spinPeriod;
        }

        //PlayFacingSpin(spinNumber, spinPeriod, spinDirection, startAngle);
    }

    void UpdateFacingFromSpinning()
    {
        if (m_spinTimer <= 0) return;

        m_spinTimer -= Time.deltaTime;
        float angleDelta = 360 / m_spinPeriod * Time.deltaTime;
        angleDelta *= m_spinDirection == SpinDirection.AntiClockwise ? 1 : -1;
        m_spinAngle += angleDelta;

        m_facing = GetFacingFromAngle(m_spinAngle);
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
                float newAngle = vy < -0.1 ? 3 * ROT : vy > 0.1 ? -ROT : ROT;
                rotation = newAngle;
            }
            else if (vx > (math.abs(vy) - 0.05))
            {
                // right
                float newAngle = vy < -0.1 ? -3 * ROT : vy > 0.1 ? ROT : -ROT;
                rotation = newAngle;
            }
        }
        else
        {
            rotation = 0;
        }

        return rotation;
    }

    float m_animUpdateTimer = 0;
    bool m_isHoldStart = false;
    bool m_isHoldReleased = false;

    void UpdateGotchiAnim()
    {
        if (IsDropSpawning) return;

        // no need to send anim update RPC's every frame
        m_animUpdateTimer -= Time.deltaTime;
        if (m_animUpdateTimer > 0) return;
        m_animUpdateTimer += 0.2f;

        var holdState = m_playerPrediction.GetHoldState();
        SetAnimatorBool("IsLeftHoldActive", holdState == PlayerPrediction.HoldState.LeftActive);
        SetAnimatorBool("IsRightHoldActive", holdState == PlayerPrediction.HoldState.RightActive);

        if (!m_isHoldStart && holdState != PlayerPrediction.HoldState.Inactive) m_isHoldStart = true;
        if (m_isHoldStart && holdState == PlayerPrediction.HoldState.Inactive) m_isHoldReleased = true;
        if (m_isHoldReleased)
        {
            ResetIdleAnimation();
            m_isHoldReleased = false;
            m_isHoldStart = false;
        }
    }

    void SetAnimatorBool(string name, bool value)
    {
        if (!IsLocalPlayer) return;

        animator.SetBool(name, value);

        SetAnimatorBoolServerRpc(name, value);
    }

    [Rpc(SendTo.Server)]
    void SetAnimatorBoolServerRpc(string name, bool value)
    {
        SetAnimatorBoolClientRpc(name, value);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SetAnimatorBoolClientRpc(string name, bool value)
    {
        if (IsLocalPlayer) return;

        animator.SetBool(name, value);
    }






}
