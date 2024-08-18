using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Dropt;
using System.Collections.Generic;
using Unity.Mathematics;

public class PlayerPrediction : NetworkBehaviour
{
    private NetworkCharacter m_networkCharacter;

    // persistent variables useful for external classes/object
    private Vector3 m_lastMoveDirection = new Vector3(0, -1, 0);
    private Vector3 m_velocity = new Vector3(0, -1, 0);

    // slow variables
    private float m_slowFactor = 1f;
    private int m_slowFactorExpiryTick = 0;
    private int m_slowFactorStartTick = 0;
    private float m_cooldownSlowFactor = 1f;

    // inputs to populate
    private Vector3 m_moveDirection;
    private Vector3 m_actionDirection = new Vector3(0, -1, 0);
    private PlayerAbilityEnum m_abilityTriggered = PlayerAbilityEnum.Null;
    private Hand m_abilityHand = Hand.Left;
    private PlayerAbilityEnum m_holdAbilityPending = PlayerAbilityEnum.Null;

    // for auto move during abilities
    //private bool m_autoMove = false;
    private Vector3 m_autoMoveVelocity = Vector3.zero;
    private int m_autoMoveExpiryTick = 0;

    // timer required so that when an action/ability is activated gotchi action 
    // direction stays the same for a short duration
    private float m_actionDirectionTimer = 0;
    private float k_actionDirectionTime = 0.5f;
    public NetworkVariable<Vector3> ActionDirection = new NetworkVariable<Vector3>();

    // for access
    private Rigidbody2D rb;
    private PlayerAbilities m_playerAbilities;
    private PlayerGotchi m_playerGotchi;

    // Netcode general
    NetworkTimer timer;
    public NetworkTimer Timer => timer;
    public const float k_serverTickRate = 10;
    const int k_bufferSize = 128;

    // Netcode client
    CircularBuffer<StatePayload> clientStateBuffer;
    CircularBuffer<InputPayload> clientInputBuffer;
    StatePayload lastServerState;

    public StatePayload LastServerState => lastServerState;
    List<StatePayload> m_lastServerStateArray;

    // Netcode server
    CircularBuffer<StatePayload> serverStateBuffer;
    Queue<InputPayload> serverInputQueue;

    // SetPlayerPosition code for teleports
    private bool m_isSetPlayerPosition = false;
    private Vector3 m_setPlayerPosition = Vector3.zero;

    [Header("Debug")]
    [SerializeField] GameObject m_clientCircle;
    [SerializeField] GameObject m_serverCircle;

    [Header("Utility")]
    [SerializeField] Camera playerCamera;

    // for calculating mouse positions
    private Vector2 m_cursorScreenPosition;
    private Vector3 m_cursorWorldPosition;

    // cooldown expiry ticks
    private int m_abilityCooldownExpiryTick = 0;
    private int m_lhSpecialCooldownExpiryTick = 0;
    private int m_rhSpecialCooldownExpiryTick = 0;

    // hold variables
    private bool m_isHoldStarted = false;
    private bool m_isHoldStartFlag = false;
    private bool m_isHoldFinishFlag = false;
    public enum HoldState { Inactive, LeftActive, RightActive };
    private HoldState m_holdState;
    private int m_holdStartTick = 0;
    private int m_holdFinishTick = 0;
    private float m_holdChargeTime = 0;
    private int m_holdInputStartTick = 0;

    // to keep track of our tick delta to the server
    private int m_remoteClientTickDelta = 0;
    List<int> m_remoteClientTickDeltas = new List<int>();

    private bool m_isDashAnimPlayed = false;

    public bool IsInputDisabled = false;

    public float MovementMultiplier = 1f;

    private PlayerController m_playerController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        m_networkCharacter = GetComponent<NetworkCharacter>();
        m_playerAbilities = GetComponent<PlayerAbilities>();
        m_playerController = GetComponent<PlayerController>();
        m_playerGotchi = GetComponent<PlayerGotchi>();
    }

    public override void OnNetworkSpawn()
    {
        // deparent our circles for representing server/client prediction locations
        if (m_clientCircle != null) m_clientCircle.transform.SetParent(null);
        if (m_serverCircle != null) m_serverCircle.transform.SetParent(null);

        // Netcode init
        timer = new NetworkTimer(k_serverTickRate);

        clientStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        clientInputBuffer = new CircularBuffer<InputPayload>(k_bufferSize);

        serverStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        serverInputQueue = new Queue<InputPayload>();

        m_remoteClientTickDelta = 0;

        m_lastServerStateArray = new List<StatePayload>();

        // transform.position is the intitial position given to us by the ConnectionApprovalHandler
        // thus we need to set all states to this position to start with
        StatePayload startState = new StatePayload { position = transform.position };
        for (int i = 0; i < k_bufferSize; i++)
        {
            serverStateBuffer.Add(startState, i);
            clientStateBuffer.Add(startState, i);
        }
    }

    public float GetSpecialCooldownRemaining(Hand hand)
    {
        if (hand == Hand.Left)
        {
            if (m_lhSpecialCooldownExpiryTick < timer.CurrentTick)
            {
                return 0;
            } else
            {
                return math.floor((m_lhSpecialCooldownExpiryTick - timer.CurrentTick) / k_serverTickRate);
            }
        } else
        {
            if (m_rhSpecialCooldownExpiryTick < timer.CurrentTick)
            {
                return 0;
            }
            else
            {
                return math.floor((m_rhSpecialCooldownExpiryTick - timer.CurrentTick) / k_serverTickRate);
            }
        }
    }

    private void SetActionDirectionAndLastMoveFromCursorAim()
    {
        m_actionDirection = math.normalizesafe(m_cursorWorldPosition - (transform.position + new Vector3(0, 0.5f, 0)));
        m_lastMoveDirection = m_actionDirection;
        m_actionDirectionTimer = k_actionDirectionTime;
    }

    private void OnMovement(InputValue value)
    {
        if (!IsLocalPlayer) return;

        if (IsInputDisabled || PlayerInputBlocker.Instance.IsMoveInputBlockerActive()) return;

        m_moveDirection = value.Get<Vector2>();


    }

    private void OnDash_CursorAim(InputValue value)
    {
        if (!IsLocalPlayer) return;

        if (IsInputDisabled || PlayerInputBlocker.Instance.IsMoveInputBlockerActive()) return;

        SetActionDirectionAndLastMoveFromCursorAim();

        m_abilityTriggered = PlayerAbilityEnum.Dash;
    }

    private void OnDash_MoveAim(InputValue value)
    {
        if (!IsLocalPlayer) return;

        if (IsInputDisabled || PlayerInputBlocker.Instance.IsMoveInputBlockerActive()) return;
        m_actionDirection = m_lastMoveDirection;
        m_actionDirectionTimer = k_actionDirectionTime;
        m_abilityTriggered = PlayerAbilityEnum.Dash;
    }

    private void OnMousePosition(InputValue value)
    {
        if (!IsLocalPlayer) return;

        m_cursorScreenPosition = value.Get<Vector2>();
    }

    private void OnLeftAttack_CursorAim(InputValue value)
    {
        if (!IsLocalPlayer) return;

        if (IsInputDisabled || PlayerInputBlocker.Instance.IsCursorWithinClickInputBlocker()) return;
        if (PlayerEquipmentDebugCanvas.IsActive()) return;


        SetActionDirectionAndLastMoveFromCursorAim();

        m_abilityHand = Hand.Left;
        var lhWearable = GetComponent<PlayerEquipment>().LeftHand.Value;
        m_abilityTriggered = m_playerAbilities.GetAttackAbilityEnum(lhWearable);
    }

    private void OnLeftHoldStart_CursorAim(InputValue value)
    {
        if (!IsLocalPlayer) return;

        if (IsInputDisabled || PlayerInputBlocker.Instance.IsCursorWithinClickInputBlocker()) return;

        var lhWearable = GetComponent<PlayerEquipment>().LeftHand.Value;
        m_holdAbilityPending = m_playerAbilities.GetHoldAbilityEnum(lhWearable);
        var holdAbility = m_playerAbilities.GetAbility(m_holdAbilityPending);

        if (holdAbility == null) return; 
        
        m_holdState = HoldState.LeftActive;
        m_holdChargeTime = holdAbility.HoldChargeTime;
        m_holdInputStartTick = timer.CurrentTick;
    }

    private void OnLeftHoldFinish_CursorAim(InputValue value)
    {
        if (!IsLocalPlayer) return;

        if (IsInputDisabled || PlayerInputBlocker.Instance.IsCursorWithinClickInputBlocker()) return;

        // if hold time > 0, trigger our hold ability
        if (m_holdState == HoldState.LeftActive)
        {
            if (!IsLocalPlayer) return;

            SetActionDirectionAndLastMoveFromCursorAim();

            m_abilityHand = Hand.Left;
            var lhWearable = GetComponent<PlayerEquipment>().LeftHand.Value;
            m_abilityTriggered = m_playerAbilities.GetHoldAbilityEnum(lhWearable);

            m_isHoldFinishFlag = true;

            m_holdAbilityPending = PlayerAbilityEnum.Null;
            m_isHoldStarted = false;
            m_holdState = HoldState.Inactive;
        }
    }

    private void OnLeftSpecial_CursorAim(InputValue value)
    {
        if (!IsLocalPlayer) return;

        if (IsInputDisabled || PlayerInputBlocker.Instance.IsCursorWithinClickInputBlocker()) return;

        SetActionDirectionAndLastMoveFromCursorAim();

        m_abilityHand = Hand.Left;
        var lhWearable = GetComponent<PlayerEquipment>().LeftHand.Value;
        m_abilityTriggered = m_playerAbilities.GetSpecialAbilityEnum(lhWearable);
    }

    private void OnRightAttack_CursorAim(InputValue value)
    {
        if (!IsLocalPlayer) return;

        if (IsInputDisabled || PlayerInputBlocker.Instance.IsCursorWithinClickInputBlocker()) return;

        SetActionDirectionAndLastMoveFromCursorAim();

        m_abilityHand = Hand.Right;
        var rhWearable = GetComponent<PlayerEquipment>().RightHand.Value;
        m_abilityTriggered = m_playerAbilities.GetAttackAbilityEnum(rhWearable);
    }

    private void OnRightHoldStart_CursorAim(InputValue value)
    {
        if (!IsLocalPlayer) return;

        if (IsInputDisabled || PlayerInputBlocker.Instance.IsCursorWithinClickInputBlocker()) return;

        var rhWearable = GetComponent<PlayerEquipment>().RightHand.Value;
        m_holdAbilityPending = m_playerAbilities.GetHoldAbilityEnum(rhWearable);
        var holdAbility = m_playerAbilities.GetAbility(m_holdAbilityPending);

        if (holdAbility == null) return;
        
        m_holdChargeTime = holdAbility.HoldChargeTime;
        m_holdState = HoldState.RightActive;
        m_holdInputStartTick = timer.CurrentTick;
    }

    private void OnRightHoldFinish_CursorAim(InputValue value)
    {
        if (!IsLocalPlayer) return;

        if (IsInputDisabled || PlayerInputBlocker.Instance.IsCursorWithinClickInputBlocker()) return;

        // if hold time > 0, trigger our hold ability
        if (m_holdState == HoldState.RightActive)
        {
            if (!IsLocalPlayer) return;

            SetActionDirectionAndLastMoveFromCursorAim();

            m_abilityHand = Hand.Right;
            var rhWearable = GetComponent<PlayerEquipment>().RightHand.Value;
            m_abilityTriggered = m_playerAbilities.GetHoldAbilityEnum(rhWearable);

            m_isHoldFinishFlag = true;

            m_holdAbilityPending = PlayerAbilityEnum.Null;
            m_isHoldStarted = false;
            m_holdState = HoldState.Inactive;
        }
    }

    private void OnRightSpecial_CursorAim(InputValue value)
    {
        if (!IsLocalPlayer) return;

        if (IsInputDisabled || PlayerInputBlocker.Instance.IsCursorWithinClickInputBlocker()) return;


        SetActionDirectionAndLastMoveFromCursorAim();

        m_abilityHand = Hand.Right;
        var rhWearable = GetComponent<PlayerEquipment>().RightHand.Value;
        m_abilityTriggered = m_playerAbilities.GetSpecialAbilityEnum(rhWearable);
    }

    public HoldState GetHoldState() { return m_holdState; }

    public float GetHoldPercentage()
    {
        if (m_holdState == HoldState.Inactive) return 0;

        var holdDuration = (timer.CurrentTick - m_holdInputStartTick) / k_serverTickRate;
        var holdPercent = math.min(holdDuration / m_holdChargeTime, 1f);

        return holdPercent;
    }

    private void UpdateCursorWorldPosition()
    {
        // Convert screen position to world position
        Vector3 screenToWorldPosition = playerCamera.ScreenToWorldPoint(
            new Vector3(m_cursorScreenPosition.x, m_cursorScreenPosition.y, Camera.main.transform.position.z));

        // Since it's a 2D game, we set the Z coordinate to 0
        m_cursorWorldPosition = new Vector3(screenToWorldPosition.x, screenToWorldPosition.y, 0);
    }

    private void Update()
    {
        // don't run if the player is dead
        if (GetComponent<PlayerController>().IsDead) return;

        // timer stuff
        float dt = Time.deltaTime;
        timer.Update(dt);
        m_actionDirectionTimer -= dt;

        //if (math.abs(m_moveDirection.x) > 0.1f || math.abs(m_moveDirection.y) > 0.1f)
        //{
        m_playerGotchi.SetMoveDirection(m_moveDirection);
        //m_playerGotchi.SetFacingFromDirection(m_moveDirection);
        //}

        // set updated render position
        if (IsLocalPlayer)
        {
            UpdateCursorWorldPosition();
            transform.position = GetLocalPlayerInterpPosition();
        }
        else if (IsClient)
        {
            transform.position = GetRemotePlayerInterpPosition();
        }

        // update debug circles if attached
        if (m_serverCircle != null) m_serverCircle.transform.position = lastServerState.position;
        if (m_clientCircle != null) m_clientCircle.transform.position = transform.position;

        // IMPORTANT: need to keep rigid body position synced for collision interactions
        // NOTE: OnTriggerEnter/Exit etc. won't work as expected when interacting with the player
        // so we must do overlap checks instead
        if (IsHost)
        {
            rb.position = lastServerState.position;
        }
    }

    // 1. See if time to do a tick
    private void FixedUpdate()
    {
        while (timer.ShouldTick())
        {
            HandleClientTick();
            HandleServerTick();
        }
    }

    // 2. Create an input payload on this tick
    void HandleClientTick()
    {
        if (!IsLocalPlayer) return;

        var currentTick = timer.CurrentTick;
        var bufferIndex = currentTick % k_bufferSize;   // this just ensures we go back to index 0 when tick goes past buffer size

        // if ability not ready, we don't count as input this tick
        var ability = m_playerAbilities.GetAbility(m_abilityTriggered);
        if (m_abilityTriggered != PlayerAbilityEnum.Null && ability == null)
        {
            Debug.LogWarning(m_abilityTriggered + " is not yet implemented or a prefab is missing");
        } 

        if (ability != null)
        {
            ability.Init(gameObject, m_abilityHand);
            bool isEnoughAp = GetComponent<NetworkCharacter>().ApCurrent.Value >= ability.ApCost;
            bool isCooldownFinished = currentTick > m_abilityCooldownExpiryTick;
            if (ability.IsSpecialAbility)
            {
                if (m_abilityHand == Hand.Left)
                {
                    isCooldownFinished = currentTick > m_lhSpecialCooldownExpiryTick;
                } else
                {
                    isCooldownFinished = currentTick > m_rhSpecialCooldownExpiryTick;
                }
            }

            if (!isEnoughAp || !isCooldownFinished)
            {
                m_abilityTriggered = PlayerAbilityEnum.Null;
            } 
        }

        // check if we can start our hold ability
        var holdAbility = m_playerAbilities.GetAbility(m_holdAbilityPending);
        if (!m_isHoldStarted && holdAbility != null)
        {
            holdAbility.Init(gameObject, m_abilityHand);
            bool isEnoughAp = GetComponent<NetworkCharacter>().ApCurrent.Value >= holdAbility.ApCost;
            bool isCooldownFinished = currentTick > m_abilityCooldownExpiryTick;

            if (isEnoughAp && isCooldownFinished)
            {
                m_isHoldStartFlag = true;
                m_isHoldStarted = true;
            }
        }

        // assemble input
        InputPayload inputPayload = new InputPayload
        {
            tick = currentTick,
            moveDirection = GetComponent<PlayerGotchi>().IsDropSpawning ? Vector3.zero : m_moveDirection * MovementMultiplier,
            actionDirection = m_actionDirection,
            abilityTriggered = m_abilityTriggered,
            holdAbilityPending = m_holdAbilityPending,
            abilityHand = m_abilityHand,
            isHoldStartFlag = m_isHoldStartFlag,
            isHoldFinishFlag = m_isHoldFinishFlag,
        };

        if (m_abilityTriggered == PlayerAbilityEnum.Dash)
        {
            if (MovementMultiplier <= 0)
            {
                m_abilityTriggered = PlayerAbilityEnum.Null;
            }
        }

        // send input to server
        SendToServerRpc(inputPayload);

        // store in client input buffer
        clientInputBuffer.Add(inputPayload, bufferIndex);

        // handle auto move
        if (ability != null && m_abilityTriggered != PlayerAbilityEnum.Null)
        {
            // we only activate once from the server side when in host mode
            if (!IsHost)
            {
                // check automove (THIS MIGHT NEED TO BE MOVED BEFORE processInput)
                if (ability.AutoMoveDuration > 0)
                {
                    //m_autoMove = true;
                    var speed = ability.AutoMoveDistance / ability.AutoMoveDuration;
                    m_autoMoveVelocity = m_actionDirection * speed;
                    m_autoMoveExpiryTick = currentTick + (int)(ability.AutoMoveDuration * k_serverTickRate);
                }
            }
        }

        // locally process the movement and save our new state for this current tick
        StatePayload statePayload = ProcessInput(inputPayload, false); // not a script simulation, use default fixed update
        clientStateBuffer.Add(statePayload, bufferIndex);

        // activate ability if it was not null
        if (ability != null && m_abilityTriggered != PlayerAbilityEnum.Null)
        {
            var holdDuration = (m_holdFinishTick - m_holdStartTick) / k_serverTickRate;

            // we only activate once from the server side when in host mode
            if (!IsHost)
            {
                ability.Activate(gameObject, statePayload, inputPayload, holdDuration);

                // set cooldown tick
                m_abilityCooldownExpiryTick = currentTick + 
                    (int)math.ceil((ability.ExecutionDuration + ability.CooldownDuration) * k_serverTickRate);

                // set special cooldown
                if (ability.IsSpecialAbility)
                {
                    int expiryTick = currentTick +
                            (int)math.ceil((ability.SpecialCooldown + ability.ExecutionDuration) * k_serverTickRate);
                    if (m_abilityHand == Hand.Left)
                    {
                        m_lhSpecialCooldownExpiryTick = expiryTick;
                    } else
                    {
                        m_rhSpecialCooldownExpiryTick = expiryTick;
                    }
                }

                // set slow down ticks
                m_slowFactor = ability.ExecutionSlowFactor;
                m_slowFactorStartTick = currentTick;
                m_slowFactorExpiryTick = currentTick + (int)math.ceil(ability.ExecutionDuration * k_serverTickRate);
                m_cooldownSlowFactor = ability.CooldownSlowFactor;
            }
        }

        // set facing
        if (m_abilityTriggered != PlayerAbilityEnum.Null)
        {
            m_playerGotchi.SetFacingFromDirection(m_actionDirection, k_actionDirectionTime, true);
            SetFacingParametersServerRpc(m_actionDirection, k_actionDirectionTime, m_lastMoveDirection);
        }

        // reset any triggers or booleans
        m_abilityTriggered = PlayerAbilityEnum.Null;
        m_isHoldStartFlag = false;
        m_isHoldFinishFlag = false;

        // do server reconciliation
        HandleServerReconciliation();

        

        // sync facing direction with remote clients
        //SetFacingParametersServerRpc(m_actionDirection, m_actionDirectionTimer, m_lastMoveDirection);

    }

    [Rpc(SendTo.Server)]
    void SetFacingParametersServerRpc(Vector3 actionDirection, float actionDirectionTimer, Vector3 lastMoveDirection)
    {
        SetFacingParametersClientRpc(actionDirection, actionDirectionTimer, lastMoveDirection);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SetFacingParametersClientRpc(Vector3 actionDirection, float actionDirectionTimer, Vector3 lastMoveDirection)
    {
        if (IsLocalPlayer) return;

        m_playerGotchi.SetFacingFromDirection(actionDirection, actionDirectionTimer, true);

        //m_actionDirection = actionDirection;
        //m_actionDirectionTimer = actionDirectionTimer;
        //m_lastMoveDirection = lastMoveDirection;
    }

    // 3. set transform to whatever the latest server state is then rewind
    void HandleServerReconciliation()
    {
        // grab the buffer index of the last server state we've received
        int bufferIndex = lastServerState.tick % k_bufferSize;
        if (bufferIndex - 1 < 0) return;    // not enough information to reconcile

        StatePayload rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState;    // host rpcs execute immedimate, so use the previous server state in the buffer

        transform.position = rewindState.position;

        clientStateBuffer.Add(rewindState, rewindState.tick);

        // replay all inputs from rewind state to current state
        int tickToReplay = lastServerState.tick + 1;

        // variables to ensure we don't get stuck in an infinite loop if the client cannot keep up
        int MAX_REPLAYS = 10;
        int counter = 0;

        while (tickToReplay <= timer.CurrentTick && counter < MAX_REPLAYS)
        {
            bufferIndex = tickToReplay % k_bufferSize;
            StatePayload statePayload = ProcessInput(clientInputBuffer.Get(bufferIndex), true);

            clientStateBuffer.Add(statePayload, bufferIndex);
            tickToReplay++;
            counter++;
        }
    }

    StatePayload ProcessInput(InputPayload input, bool isReconciliation = false)
    {
        // set starting position and velocity
        if (IsLocalPlayer)
        {
            // if normal flow, use previous states position as start transform
            if (!isReconciliation)
            {
                // get the previous ticks final state as transform position for this movement
                var prevBufferIndex = (input.tick - 1) % k_bufferSize;
                transform.position = clientStateBuffer.Get(prevBufferIndex).position;
            }
            // else if reconciliation time, do nothing, we start with the rewind state tranform position
        }
        if (IsServer)
        {
            // get the previous ticks final state as transform position for this movement
            var prevBufferIndex = (input.tick - 1) % k_bufferSize;
            transform.position = serverStateBuffer.Get(prevBufferIndex).position;
        }

        // handle any hold info
        if (input.isHoldStartFlag) m_holdStartTick = input.tick;
        if (input.isHoldFinishFlag) m_holdFinishTick = input.tick;

        // teleport handling
        HandleTeleportInput(input);

        // set velocity
        if (isReconciliation)
        {
            // in reconciliation mode we need to grab the velocities stored in the client state buffer
            // rather than try generate them again
            var bufferIndex = input.tick % k_bufferSize;
            rb.velocity = clientStateBuffer.Get(bufferIndex).velocity;
        } else
        {
            // generate velocity from char speed, move dir any potential abilities that slow down speed
            rb.velocity = input.moveDirection * m_networkCharacter.MoveSpeed.Value * 
                GetInputSlowFactor(input);

            // check for automove
            if (input.tick < m_autoMoveExpiryTick)
            {
                rb.velocity = m_autoMoveVelocity;
            }

            // check for teleport
            var ability = m_playerAbilities.GetAbility(input.abilityTriggered);
            if (ability != null)
            {
                if (ability.TeleportDistance > 0.1f)
                {
                    rb.velocity = Vector3.zero;
                }
            }
        }

        var stateVelocity = rb.velocity;


        // simulate
        Physics2D.simulationMode = SimulationMode2D.Script;
        var simulationTime = 1f / k_serverTickRate;
        while (simulationTime > 0f)
        {
            Physics2D.Simulate(Time.fixedDeltaTime);    // need to simulate at fixedDeltaTime
            simulationTime -= Time.fixedDeltaTime;
        }
        rb.velocity = Vector3.zero;
        rb.position = transform.position;   // CHECK THIS
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;

        // update private variables
        if (input.moveDirection.x != 0 || input.moveDirection.y != 0) m_lastMoveDirection = input.moveDirection;
        m_velocity = input.moveDirection * m_networkCharacter.MoveSpeed.Value;

        return new StatePayload
        {
            tick = input.tick,
            position = transform.position,
            velocity = stateVelocity,
            abilityTriggered = input.abilityTriggered,
        };
    }

    public void HandleTeleportInput(InputPayload input)
    {
        var ability = m_playerAbilities.GetAbility(input.abilityTriggered);
        if (ability != null && ability.TeleportDistance > 0.1f)
        {
            transform.position = DashCalcs.Dash(GetComponent<CapsuleCollider2D>(), transform.position,
                input.actionDirection, ability.TeleportDistance);
        }
    }

    public float GetInputSlowFactor(InputPayload input)
    {
        if (input.tick >= m_slowFactorStartTick && input.tick <= m_slowFactorExpiryTick)
        {
            return m_slowFactor;
        } else if (input.tick <= m_abilityCooldownExpiryTick)
        {
            return m_cooldownSlowFactor;
        }
        else if (input.tick >= m_holdStartTick && input.holdAbilityPending != PlayerAbilityEnum.Null)
        {
            var holdAbility = m_playerAbilities.GetAbility(input.holdAbilityPending);
            return holdAbility.HoldSlowFactor;
        }
        else
        {
            return 1;
        }
    }

    public void SetPlayerPosition(Vector3 position)
    {
        if (!IsServer) return;

        m_isSetPlayerPosition = true;
        m_setPlayerPosition = position;
    }

    void HandleServerTick()
    {
        if (!IsServer) return;

        var bufferIndex = -1;
        InputPayload inputPayload = default;
        StatePayload statePayload = default;

        while (serverInputQueue.Count > 0)
        {
            // 1. get the oldest input
            inputPayload = serverInputQueue.Dequeue();

            // 2. check if ability triggered
            var ability = m_playerAbilities.GetAbility(inputPayload.abilityTriggered);
            if (ability != null)
            {
                ability.Init(gameObject, inputPayload.abilityHand);
                bool isApEnough = GetComponent<NetworkCharacter>().ApCurrent.Value >= ability.ApCost;
                bool isCooldownFinished = inputPayload.tick > m_abilityCooldownExpiryTick;

                if (ability.IsSpecialAbility)
                {
                    if (m_abilityHand == Hand.Left)
                    {
                        isCooldownFinished = inputPayload.tick > m_lhSpecialCooldownExpiryTick;
                    }
                    else
                    {
                        isCooldownFinished = inputPayload.tick > m_rhSpecialCooldownExpiryTick;
                    }
                }

                if ((!isApEnough || !isCooldownFinished) && !IsHost)
                {
                    inputPayload.abilityTriggered = PlayerAbilityEnum.Null;
                }
            }

            // 3. check if we can start our hold ability
            var holdAbility = m_playerAbilities.GetAbility(inputPayload.holdAbilityPending);
            if (!m_isHoldStarted && inputPayload.isHoldStartFlag && holdAbility != null && !IsHost)
            {
                holdAbility.Init(gameObject, inputPayload.abilityHand);
                bool isEnoughAp = GetComponent<NetworkCharacter>().ApCurrent.Value >= holdAbility.ApCost;
                bool isCooldownFinished = inputPayload.tick > m_abilityCooldownExpiryTick;

                if (isEnoughAp && isCooldownFinished)
                {
                    m_isHoldStarted = true;
                    inputPayload.isHoldStartFlag = true;
                } else
                {
                    inputPayload.isHoldStartFlag = false;
                }
            }


            // 4. handle auto-move
            if (ability != null && inputPayload.abilityTriggered != PlayerAbilityEnum.Null)
            {
                // check automove (THIS MIGHT NEED TO BE MOVED BEFORE processInput)
                if (ability.AutoMoveDuration > 0)
                {
                    //m_autoMove = true;
                    var speed = ability.AutoMoveDistance / ability.AutoMoveDuration;
                    m_autoMoveVelocity = inputPayload.actionDirection * speed;
                    m_autoMoveExpiryTick = inputPayload.tick + (int)(ability.AutoMoveDuration * k_serverTickRate);
                }
            }

            // 5. process input
            statePayload = ProcessInput(inputPayload, false);

            // 6. set position if it has been triggered (level teleportation to player spawns)
            if (m_isSetPlayerPosition)
            {
                statePayload.position = m_setPlayerPosition;
            }

            // 7. add to the server state buffer
            bufferIndex = inputPayload.tick % k_bufferSize;
            serverStateBuffer.Add(statePayload, bufferIndex);

            // 8. perform ability if applicable
            if (ability != null && inputPayload.abilityTriggered != PlayerAbilityEnum.Null)
            {
                var holdDuration = (m_holdFinishTick - m_holdStartTick) / k_serverTickRate;
                ability.Activate(gameObject, statePayload, inputPayload, holdDuration);
                m_abilityCooldownExpiryTick = inputPayload.tick + 
                    (int)math.ceil((ability.ExecutionDuration + ability.CooldownDuration) * k_serverTickRate);

                // set special cooldown
                if (ability.IsSpecialAbility)
                {
                    int expiryTick = inputPayload.tick +
                            (int)math.ceil((ability.SpecialCooldown + ability.ExecutionDuration) * k_serverTickRate);
                    if (m_abilityHand == Hand.Left)
                    {
                        m_lhSpecialCooldownExpiryTick = expiryTick;
                    }
                    else
                    {
                        m_rhSpecialCooldownExpiryTick = expiryTick;
                    }
                }

                // set slow down ticks
                m_slowFactor = ability.ExecutionSlowFactor;
                m_slowFactorStartTick = inputPayload.tick;
                m_slowFactorExpiryTick = inputPayload.tick + 
                    (int)math.ceil(ability.ExecutionDuration * k_serverTickRate);
                m_cooldownSlowFactor = ability.CooldownSlowFactor;
            }

            // 9. tell client the last state we have as a server
            SendToClientRpc(statePayload);

            // 10. resest player inactive state (if we moved or ability triggered)
            if (ability != null || inputPayload.moveDirection.magnitude > 0.01)
            {
                m_playerController.ResetInactiveTimer();
            }
        }

        // reset state of setting player position
        m_isSetPlayerPosition = false;
    }

    private bool m_isRemoteClientTickDeltaSet = false;

    // this function executed on CLIENT
    [ClientRpc]
    void SendToClientRpc(StatePayload statePayload)
    {
        // save last server state
        lastServerState = statePayload;

        // append state to last server state array
        m_lastServerStateArray.Add(statePayload);
        if (m_lastServerStateArray.Count > 10) m_lastServerStateArray.RemoveAt(0);

        // get average delta tick
        if (!m_isRemoteClientTickDeltaSet)
        {
            var deltaTick = timer.CurrentTick - statePayload.tick;
            m_remoteClientTickDeltas.Add(deltaTick);
            if (m_remoteClientTickDeltas.Count >= 10)
            {
                float sum = 0;
                foreach (var delta in m_remoteClientTickDeltas) sum += delta;
                m_remoteClientTickDelta = (int)math.round(sum/m_remoteClientTickDeltas.Count);
                m_isRemoteClientTickDeltaSet = true;
            }
        }

        //var deltaTick = timer.CurrentTick - statePayload.tick;
        //m_remoteClientTickDeltas.Add(deltaTick);
        //if (m_remoteClientTickDeltas.Count > 10) m_remoteClientTickDeltas.RemoveAt(0);
        //float sum = 0;
        //foreach (var delta in m_remoteClientTickDeltas) sum += delta;
        //m_remoteClientTickDelta = (int)math.round(sum / m_remoteClientTickDeltas.Count);
    }

    // this function executed on SERVER
    [Rpc(SendTo.Server)]
    void SendToServerRpc(InputPayload input)
    {
        serverInputQueue.Enqueue(input);
    }

    public Vector3 GetRemotePlayerInterpPosition()
    {
        //return lastServerState.position;
        if (m_lastServerStateArray.Count < 5) return transform.position;

        var targetTick = timer.CurrentTickAndFraction.Tick - m_remoteClientTickDelta - 3;

        // find out where we are in last server state array
        int a = 0;
        int b = 0;

        for (int i = 0; i < m_lastServerStateArray.Count - 1; i++)
        {
            if (targetTick == m_lastServerStateArray[i].tick)
            {
                a = i;
                b = i + 1;
                break;
            }
        }

        // something went wrong so just return original position
        if (a == 0 && b == 0)
        {
            Debug.Log("Remote player outside interp range");
            return transform.position;
        }

        // store interp values
        var start = m_lastServerStateArray[a];
        var finish = m_lastServerStateArray[b];
        var fraction = timer.CurrentTickAndFraction.Fraction;

        // Draw dash shadow if we dashed
        IfDashInputDrawShadow(start, finish);

        // go straight to finish if there was a teleport
        var ability = m_playerAbilities.GetAbility(finish.abilityTriggered);
        if (ability != null && ability.TeleportDistance > 0.1f) return finish.position;
        return Vector3.Lerp(start.position, finish.position, fraction);
    }

    public Vector3 GetLocalPlayerInterpPosition()
    {
        // go back at least two ticks for our interp state
        if (timer.CurrentTick < 3) return transform.position;

        var currTick = timer.CurrentTickAndFraction.Tick;

        var startBufferIndex = (currTick - 2) % k_bufferSize;
        var finishBufferIndex = (currTick - 1) % k_bufferSize;

        var start = clientStateBuffer.Get(startBufferIndex);
        var finish = clientStateBuffer.Get(finishBufferIndex);
        var fraction = timer.CurrentTickAndFraction.Fraction;

        // Draw dash shadow if we dashed
        IfDashInputDrawShadow(start, finish);

        // go straight to finish if there was a teleport
        var ability = m_playerAbilities.GetAbility(finish.abilityTriggered);
        if (ability != null && ability.TeleportDistance > 0.1f) return finish.position;
        return Vector3.Lerp(start.position, finish.position, fraction);
    }

    public Vector3 GetInterpPositionAtTick(int tick)
    {
        // because we do interpolation between tick-2 and tick-1, for simplicity (and some determinism)
        // lets take the mid point between those two positions
        var startBufferIndex = (tick-2) % k_bufferSize;
        var finishBufferIndex = (tick-1) % k_bufferSize;

        if (IsLocalPlayer)
        {
            var posA = clientStateBuffer.Get(startBufferIndex).position;
            var posB = clientStateBuffer.Get(finishBufferIndex).position;
            return math.lerp(posA, posB, 0.5f);
        }
        else
        {
            var posA = serverStateBuffer.Get(startBufferIndex).position;
            var posB = serverStateBuffer.Get(finishBufferIndex).position;
            return math.lerp(posA, posB, 0.5f);
        }
    }

    void IfDashInputDrawShadow(StatePayload start, StatePayload finish)
    {
        var ability = m_playerAbilities.GetAbility(finish.abilityTriggered);
        if (ability != null && ability.TeleportDistance > 0.1f)
        {
            // play dash anim
            if (!m_isDashAnimPlayed)
            {
                var offset = Vector3.zero;
                if (finish.abilityTriggered == PlayerAbilityEnum.PierceLance) offset.y = 5f;

                gameObject.GetComponentInChildren<DashTrailSpawner>().DrawShadow(
                    start.position, 
                    finish.position + offset, 
                    (int)math.ceil(ability.TeleportDistance)+1);
                m_isDashAnimPlayed = true;
            }
        }
        else
        {
            m_isDashAnimPlayed = false;
        }
    }

    public Vector3 GetServerPosition()
    {
        return lastServerState.position;
    }

    //public Vector3 GetFacingDirection()
    //{
    //    if (m_actionDirectionTimer > 0)
    //    {
    //        return math.normalize(m_actionDirection);
    //    }
    //    else
    //    {
    //        return m_lastMoveDirection;
    //    }
    //}

    //public bool IsMoving { get
    //    {
    //        return math.abs(m_velocity.x) > 0.1f || math.abs(m_velocity.y) > 0.1f;
    //    }
    //    private set { }
    //}

    public float GetServerTickRate()
    {
        return k_serverTickRate;
    }
}