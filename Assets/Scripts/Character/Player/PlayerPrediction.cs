using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Dropt;
using System.Collections.Generic;
using Unity.Mathematics;

public partial class PlayerPrediction : NetworkBehaviour
{
    private NetworkCharacter m_networkCharacter;

    // slow variables
    private float m_slowFactor = 1f;
    private int m_slowFactorExpiryTick = 0;
    private int m_slowFactorStartTick = 0;
    private float m_cooldownSlowFactor = 1f;

    // inputs to populate
    private Vector3 m_moveDirection;
    private Vector3 m_lastNonZeroMoveDirection = new Vector3(0, -1, 0);
    private Vector3 m_actionDirection = new Vector3(0, -1, 0);
    private float m_actionDistance = 0;
    private AttackCentre m_playerAttackCentre;
    private PlayerAbilityEnum m_triggeredAbilityEnum = PlayerAbilityEnum.Null;
    private Hand m_abilityHand = Hand.Left;
    private PlayerAbilityEnum m_holdStartTriggeredAbilityEnum = PlayerAbilityEnum.Null;

    private PlayerTargetingReticle m_playerTargetingReticle;

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

    // hold variables
    //private bool m_isHoldStarted = false;
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

    public bool IsInputEnabled = false;
    public bool IsMovementEnabled = true;
    public bool IsActionsEnabled = true;

    public float MovementMultiplier = 1f;

    private PlayerController m_playerController;

    private PlayerInput m_playerInput;
    private InputAction m_movementAction;  // Reference to the "Movement" action

    public bool IsInteracting = false;
    public bool IsFreezeMovementWhileTargeting = false;
    private bool m_IsShieldAbilityActive;

    private PlayerEquipment m_playerEquipment;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        m_networkCharacter = GetComponent<NetworkCharacter>();
        m_playerAbilities = GetComponent<PlayerAbilities>();
        m_playerController = GetComponent<PlayerController>();
        m_playerGotchi = GetComponent<PlayerGotchi>();

        m_playerInput = GetComponent<PlayerInput>();
        m_movementAction = m_playerInput.actions["Generic_PlayerMove"];
        m_playerAttackCentre = GetComponentInChildren<AttackCentre>();
        m_playerTargetingReticle = GetComponent<PlayerTargetingReticle>();
        m_playerEquipment = GetComponent<PlayerEquipment>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // deparent our circles for representing server/client prediction locations
        if (m_clientCircle != null) m_clientCircle.transform.SetParent(null);
        if (m_serverCircle != null) m_serverCircle.transform.SetParent(null);

        clientStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        clientInputBuffer = new CircularBuffer<InputPayload>(k_bufferSize);

        serverStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        serverInputQueue = new Queue<InputPayload>();


        m_lastServerStateArray = new List<StatePayload>();

        ResetRemoteClientTickDelta();

        // transform.position is the intitial position given to us by the ConnectionApprovalHandler
        // thus we need to set all states to this position to start with
        StatePayload startState = new StatePayload { position = transform.position };
        for (int i = 0; i < k_bufferSize; i++)
        {
            serverStateBuffer.Add(startState, i);
            clientStateBuffer.Add(startState, i);
        }

        // start input disabled
        if (IsClient) IsInputEnabled = false;
    }

    void ResetRemoteClientTickDelta()
    {
        m_remoteClientTickDelta = 0;
        m_isRemoteClientTickDeltaSet = false;
        m_remoteClientTickDeltas.Clear();
    }

    public float GetSpecialCooldownRemaining(Hand hand)
    {
        if (m_playerEquipment == null) return 0;

        if (hand == Hand.Left)
        {
            var lhWearableEnum = m_playerEquipment.LeftHand.Value;
            var specialAbilityEnum = m_playerAbilities.GetSpecialAbilityEnum(lhWearableEnum);
            var specialAbility = m_playerAbilities.GetAbility(specialAbilityEnum);
            return specialAbility != null ? specialAbility.GetCooldownRemaining() : 0;
        }
        else
        {
            var rhWearableEnum = m_playerEquipment.RightHand.Value;
            var specialAbilityEnum = m_playerAbilities.GetSpecialAbilityEnum(rhWearableEnum);
            var specialAbility = m_playerAbilities.GetAbility(specialAbilityEnum);
            return specialAbility != null ? specialAbility.GetCooldownRemaining() : 0;
        }
    }

    public HoldState GetHoldState() { return m_holdState; }

    public float GetHoldPercentage()
    {
        int currentTick = NetworkTimer_v2.Instance.TickCurrent;
        float tickRate = NetworkTimer_v2.Instance.TickRate;

        if (m_holdState == HoldState.Inactive || m_IsShieldAbilityActive) return 0;

        var holdDuration = (currentTick - m_holdInputStartTick) / tickRate;
        var holdPercent = math.min(holdDuration / m_holdChargeTime, 1f);

        return holdPercent;
    }

    private void Update()
    {
        // don't run if the player is dead
        if (GetComponent<PlayerController>().IsDead) return;

        // timer stuff
        float dt = Time.deltaTime;
        //timer.Update(dt);
        m_actionDirectionTimer -= dt;

        // update playerGotchi on the current move direction
        m_playerGotchi.SetMoveDirection(m_moveDirection);

        // set updated render position
        if (IsLocalPlayer)
        {
            // update input from our PlayerPrediction_Input.cs file
            UpdateInput();

            // update position
            transform.position = GetLocalPlayerInterpPosition();
        }
        else if (IsClient && !IsLocalPlayer)
        {
            transform.position = GetRemotePlayerInterpPosition();
            //Debug.Log(NetworkTimer_v2.Instance.TickFraction + " " + transform.position);
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

    // NEW: We now let the NetworkTimer_v2 call the Tick function for us
    public void Tick()
    {
        HandleClientTick();
        HandleServerTick();
    }

    // 2. Create an input payload on this tick
    void HandleClientTick()
    {
        // do this only for local players
        if (!IsLocalPlayer) return;

        // store current tick and buffer index
        var currentTick = NetworkTimer_v2.Instance.TickCurrent;
        var tickRate = NetworkTimer_v2.Instance.TickRate;
        var bufferIndex = currentTick % k_bufferSize;   // this just ensures we go back to index 0 when tick goes past buffer size

        // if ability not ready, we don't count as input this tick
        var triggeredAbility = m_playerAbilities.GetAbility(m_triggeredAbilityEnum);
        if (triggeredAbility != null)
        {
            // check ap and cooldown (we ignore cooldown for hold abilities)
            bool isApEnough = m_networkCharacter.ApCurrent.Value >= triggeredAbility.ApCost;
            bool isCooldownFinished = triggeredAbility.IsCooldownFinished();
            bool isBlockedByOthers = false;

            // check if blocked by others
            if (m_triggeredAbilityEnum != PlayerAbilityEnum.Dash)
            {
                isBlockedByOthers = !IsAttackCooldownFinished(Hand.Left) || !IsAttackCooldownFinished(Hand.Right) ||
                    !IsHoldCooldownFinished(Hand.Left) || !IsHoldCooldownFinished(Hand.Right);
            }

            if (isApEnough && isCooldownFinished && !isBlockedByOthers)
            {
                triggeredAbility.Init(gameObject, m_abilityHand);
            }
            else
            {
                m_triggeredAbilityEnum = PlayerAbilityEnum.Null;
                triggeredAbility = null;
            }
        }

        // check if we can init and hold start our hold ability
        var holdStartTriggeredAbility = m_playerAbilities.GetAbility(m_holdStartTriggeredAbilityEnum);
        if (holdStartTriggeredAbility != null)
        {
            // check AP only, we can't check against cooldown because we are commencing this attack within the starter attacks cooldown window
            bool isEnoughAp = m_networkCharacter.ApCurrent.Value >= holdStartTriggeredAbility.ApCost;
            if (isEnoughAp)
            {
                holdStartTriggeredAbility.Init(gameObject, m_abilityHand);
                holdStartTriggeredAbility.HoldStart();
                m_isHoldStartFlag = true;
               
            }
            else
            {
                m_holdStartTriggeredAbilityEnum = PlayerAbilityEnum.Null;
                holdStartTriggeredAbility = null;
            }
        }

        // assemble input
        InputPayload inputPayload = new InputPayload
        {
            tick = currentTick,
            moveDirection = m_playerGotchi.IsDropSpawning ? Vector3.zero : m_moveDirection * MovementMultiplier,
            actionDirection = m_actionDirection,
            actionDistance = m_actionDistance,
            triggeredAbilityEnum = m_triggeredAbilityEnum,
            holdStartTriggeredAbilityEnum = m_holdStartTriggeredAbilityEnum,
            abilityHand = m_abilityHand,                            // ability hand is set in the client input On functions
            isHoldStartFlag = m_isHoldStartFlag,                    // hold start flag is set if hold start ability triggered from client input AND ap and cooldown is sufficient
            isHoldFinishFlag = m_isHoldFinishFlag,                  // hold finish flag is set by the hold finish client input
            isMovementEnabled = IsMovementEnabled,
        };

        // send input to server
        SendToServerRpc(inputPayload);

        // store in client input buffer
        clientInputBuffer.Add(inputPayload, bufferIndex);

        // handle auto move
        if (triggeredAbility != null && m_triggeredAbilityEnum != PlayerAbilityEnum.Null
            && triggeredAbility.AutoMoveDuration > 0)
        {
            var speed = triggeredAbility.AutoMoveDistance / triggeredAbility.AutoMoveDuration;
            m_autoMoveVelocity = m_actionDirection * speed;
            m_autoMoveExpiryTick = currentTick + (int)(triggeredAbility.AutoMoveDuration * tickRate);
        }

        // locally process the movement and save our new state for this current tick
        StatePayload statePayload = ProcessInput(inputPayload, false, false); // not a script simulation, use default fixed update
        clientStateBuffer.Add(statePayload, bufferIndex);

        // activate ability if it was not null
        if (triggeredAbility != null && m_triggeredAbilityEnum != PlayerAbilityEnum.Null)
        {
            // calc any hold duration
            var holdDuration = (m_holdFinishTick - m_holdStartTick) / tickRate;

            // call HoldFinish() if this is a hold ability
            if (triggeredAbility.abilityType == PlayerAbility.AbilityType.Hold) triggeredAbility.HoldFinish();

            // activate ability
            triggeredAbility.Activate(gameObject, statePayload, inputPayload, holdDuration);

            // set slow down ticks
            m_slowFactor = triggeredAbility.ExecutionSlowFactor;
            m_slowFactorStartTick = currentTick;
            m_slowFactorExpiryTick = currentTick + (int)math.ceil(triggeredAbility.ExecutionDuration * tickRate);
            m_cooldownSlowFactor = triggeredAbility.CooldownSlowFactor;
        }

        // set facing
        if (m_triggeredAbilityEnum != PlayerAbilityEnum.Null)
        {
            m_playerGotchi.SetFacingFromDirection(m_actionDirection, k_actionDirectionTime, true);
            SetFacingParametersServerRpc(m_actionDirection, k_actionDirectionTime, m_lastNonZeroMoveDirection);
        }

        // reset any triggers or booleans
        m_triggeredAbilityEnum = PlayerAbilityEnum.Null;
        //m_holdStartTriggeredAbilityEnum = PlayerAbilityEnum.Null;
        m_isHoldStartFlag = false;
        m_isHoldFinishFlag = false;

        // do server reconciliation
        HandleServerReconciliation();
    }

    void HandleServerTick()
    {
        if (!IsServer) return;

        var bufferIndex = -1;
        InputPayload inputPayload = default;
        StatePayload statePayload = default;

        var tickRate = NetworkTimer_v2.Instance.TickRate;

        while (serverInputQueue.Count > 0)
        {
            // 1. get the oldest input
            inputPayload = serverInputQueue.Dequeue();

            // 2. check if ability triggered
            var triggeredAbility = m_playerAbilities.GetAbility(inputPayload.triggeredAbilityEnum);
            if (triggeredAbility != null && !IsHost)
            {
                // check ap and cooldown sufficient
                bool isApEnough = m_networkCharacter.ApCurrent.Value >= triggeredAbility.ApCost;
                bool isCooldownFinished = triggeredAbility.IsCooldownFinished();
                bool isBlockedByOthers = false;

                // check if blocked by others
                if (inputPayload.triggeredAbilityEnum != PlayerAbilityEnum.Dash)
                {
                    isBlockedByOthers = !IsAttackCooldownFinished(Hand.Left) || !IsAttackCooldownFinished(Hand.Right) ||
                        !IsHoldCooldownFinished(Hand.Left) || !IsHoldCooldownFinished(Hand.Right);
                }

                if (isApEnough && isCooldownFinished && !isBlockedByOthers)
                {
                    if (!IsHost) triggeredAbility.Init(gameObject, inputPayload.abilityHand);
                }
                else
                {
                    inputPayload.triggeredAbilityEnum = PlayerAbilityEnum.Null;
                }
            }

            // 3. check if hold ability started
            var holdStartTriggeredAbility = m_playerAbilities.GetAbility(inputPayload.holdStartTriggeredAbilityEnum);
            if (holdStartTriggeredAbility != null && !IsHost)
            {
                // check AP only, we can't check against cooldown because we are commencing this attack within the starter attacks cooldown window
                bool isEnoughAp = m_networkCharacter.ApCurrent.Value >= holdStartTriggeredAbility.ApCost;
                if (isEnoughAp)
                {
                    if (!IsHost) holdStartTriggeredAbility.Init(gameObject, inputPayload.abilityHand);
                    if (!IsHost) holdStartTriggeredAbility.HoldStart();
                    inputPayload.isHoldStartFlag = true;
                }
                else
                {
                    inputPayload.isHoldStartFlag = false;
                    inputPayload.holdStartTriggeredAbilityEnum = PlayerAbilityEnum.Null;
                    holdStartTriggeredAbility = null;
                }
            }


            // 4. handle auto-move
            if (triggeredAbility != null && inputPayload.triggeredAbilityEnum != PlayerAbilityEnum.Null && triggeredAbility.AutoMoveDuration > 0 && !IsHost)
            {
                var speed = triggeredAbility.AutoMoveDistance / triggeredAbility.AutoMoveDuration;
                m_autoMoveVelocity = inputPayload.actionDirection * speed;
                m_autoMoveExpiryTick = inputPayload.tick + (int)(triggeredAbility.AutoMoveDuration * tickRate);
            }

            // 5. process input
            statePayload = ProcessInput(inputPayload, false, true);

            // 6. set position if it has been triggered (level teleportation to player spawns)
            if (m_isSetPlayerPosition)
            {
                statePayload.position = m_setPlayerPosition;
                rb.position = m_setPlayerPosition;
                m_isSetPlayerPositionCounter++;
            }

            // 7. add to the server state buffer
            bufferIndex = inputPayload.tick % k_bufferSize;
            serverStateBuffer.Add(statePayload, bufferIndex);

            // 8. perform ability if applicable
            if (triggeredAbility != null && inputPayload.triggeredAbilityEnum != PlayerAbilityEnum.Null && !IsHost)
            {
                // calc any hold duration
                var holdDuration = (m_holdFinishTick - m_holdStartTick) / tickRate;

                // call HoldFinish() if this is a hold ability
                if (!IsHost && triggeredAbility.abilityType == PlayerAbility.AbilityType.Hold) triggeredAbility.HoldFinish();

                // activate
                if (!IsHost) triggeredAbility.Activate(gameObject, statePayload, inputPayload, holdDuration);

                if (!IsHost)
                {
                    // set slow down ticks
                    m_slowFactor = triggeredAbility.ExecutionSlowFactor;
                    m_slowFactorStartTick = inputPayload.tick;
                    m_slowFactorExpiryTick = inputPayload.tick + (int)math.ceil(triggeredAbility.ExecutionDuration * tickRate);
                    m_cooldownSlowFactor = triggeredAbility.CooldownSlowFactor;
                }

            }

            // 9. tell client the last state we have as a server
            SendToClientRpc(statePayload);

            //// 10. resest player inactive state (if we moved or ability triggered)
            //if (triggeredAbility != null || inputPayload.moveDirection.magnitude > 0.01)
            //{
            //    m_playerController.ResetInactiveTimer();
            //}
        }

        // reset state of setting player position
        if (m_isSetPlayerPositionCounter > 5)
        {
            m_isSetPlayerPosition = false;
        }
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

        var currentTick = NetworkTimer_v2.Instance.TickCurrent;

        while (tickToReplay <= currentTick && counter < MAX_REPLAYS)
        {
            bufferIndex = tickToReplay % k_bufferSize;
            StatePayload statePayload = ProcessInput(clientInputBuffer.Get(bufferIndex), true, false);

            clientStateBuffer.Add(statePayload, bufferIndex);
            tickToReplay++;
            counter++;
        }
    }

    StatePayload ProcessInput(InputPayload input, bool isReconciliation, bool isServerCalling)
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
        }
        else
        {
            // generate velocity from char speed, move dir any potential abilities that slow down speed
            rb.velocity = input.moveDirection * m_networkCharacter.MoveSpeed.Value *
                GetInputSlowFactor(input, isServerCalling);

            // check for automove
            if (input.tick < m_autoMoveExpiryTick)
            {
                rb.velocity = m_autoMoveVelocity;
            }

            // check for teleport
            var ability = m_playerAbilities.GetAbility(input.triggeredAbilityEnum);
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
        var simulationTime = 1f / NetworkTimer_v2.Instance.TickRate;
        while (simulationTime > 0f)
        {
            Physics2D.Simulate(Time.fixedDeltaTime);    // need to simulate at fixedDeltaTime
            simulationTime -= Time.fixedDeltaTime;
        }
        rb.velocity = Vector3.zero;
        rb.position = transform.position;   // CHECK THIS
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;

        return new StatePayload
        {
            tick = input.tick,
            position = transform.position,
            velocity = stateVelocity,
            abilityTriggered = input.triggeredAbilityEnum,
        };
    }

    public void HandleTeleportInput(InputPayload input)
    {
        if (!input.isMovementEnabled) return;

        var ability = m_playerAbilities.GetAbility(input.triggeredAbilityEnum);
        if (ability != null && ability.TeleportDistance > 0.1f)
        {
            transform.position = DashCalcs.Dash(GetComponent<CapsuleCollider2D>(), transform.position,
                input.actionDirection, ability.TeleportDistance);
        }
    }

    public float GetInputSlowFactor(InputPayload input, bool isServerCalling)
    {
        // adjusting "input.tick >" to "input.tick >=" can make a big difference in terms of glitchiness in host mode
        if (input.tick > m_slowFactorStartTick && input.tick <= m_slowFactorExpiryTick)
        {
            return m_slowFactor;
        }
        else if (input.tick >= m_holdStartTick && input.holdStartTriggeredAbilityEnum != PlayerAbilityEnum.Null)
        {
            var holdAbility = m_playerAbilities.GetAbility(input.holdStartTriggeredAbilityEnum);
            return holdAbility.HoldSlowFactor;
        }
        // NOTE FOR NOW WE ARE COMPLETELY IGNORING COOLDOWN SLOW FACTOR

        else
        {
            return 1;
        }
    }

    private int m_isSetPlayerPositionCounter = 0;

    public void SetPlayerPosition(Vector3 position)
    {
        if (!IsServer) return;

        m_isSetPlayerPosition = true;
        m_isSetPlayerPositionCounter = 0;
        m_setPlayerPosition = position;
    }

    private bool m_isRemoteClientTickDeltaSet = false;
    public void SetIsRemoteClientTickDeltaSet(bool isSet) { m_isRemoteClientTickDeltaSet = isSet; }

    private bool isFirstSet = true;

    // this function executed on CLIENT
    [ClientRpc]
    void SendToClientRpc(StatePayload statePayload)
    {
        // save last server state
        lastServerState = statePayload;

        var currentTick = NetworkTimer_v2.Instance.TickCurrent;
        var tickRate = NetworkTimer_v2.Instance.TickRate;

        // append state to last server state array, only store 2 seconds of tick data
        m_lastServerStateArray.Add(statePayload);
        if (m_lastServerStateArray.Count > tickRate * 2) m_lastServerStateArray.RemoveAt(0);

        // track tick deltas
        var deltaTick = currentTick - statePayload.tick;
        m_remoteClientTickDeltas.Add(deltaTick);
        if (m_remoteClientTickDeltas.Count > 10) m_remoteClientTickDeltas.RemoveAt(0);

        // get average
        float sum = 0;
        foreach (var delta in m_remoteClientTickDeltas) sum += delta;

        // if new remote client tick delta is 5 or more different from the old, replace the old delta
        int newRemoteClientTickDelta = (int)math.round(sum / m_remoteClientTickDeltas.Count);
        if (math.abs(m_remoteClientTickDelta - newRemoteClientTickDelta) > 5 || isFirstSet)
        {
            m_remoteClientTickDelta = newRemoteClientTickDelta;
            isFirstSet = false;
        }
    }

    // this function executed on SERVER
    [Rpc(SendTo.Server)]
    void SendToServerRpc(InputPayload input)
    {
        serverInputQueue.Enqueue(input);
    }

    public Vector3 GetRemotePlayerInterpPosition()
    {
        var currentTick = NetworkTimer_v2.Instance.TickCurrent;

        //return lastServerState.position;
        if (m_lastServerStateArray.Count < 5) return transform.position;

        // sort the array in case ticks were received out of order
        m_lastServerStateArray.Sort((state1, state2) => state1.tick.CompareTo(state2.tick));

        var targetTick = currentTick - m_remoteClientTickDelta - 10;

        // find out where we are in last server state array
        int a = -1;
        int b = -1;

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
        if (a == -1 || b == -1)
        {
            Debug.Log("Remote player outside interp range. Target tick: " + targetTick +
                ", LastServerOldest Tick: " + m_lastServerStateArray[0].tick + ", LastServerNewest Tick: " + m_lastServerStateArray[m_lastServerStateArray.Count - 1].tick);
            return transform.position;
        }

        // store interp values
        var start = m_lastServerStateArray[a];
        var finish = m_lastServerStateArray[b];
        var fraction = NetworkTimer_v2.Instance.TickFraction;

        // Draw dash shadow if we dashed
        IfDashInputDrawShadow(start, finish);

        // go straight to finish if there was a teleport
        var ability = m_playerAbilities.GetAbility(finish.abilityTriggered);
        if (ability != null && ability.TeleportDistance > 0.1f) return finish.position;
        return Vector3.Lerp(start.position, finish.position, fraction);
    }

    public Vector3 GetLocalPlayerInterpPosition()
    {
        var currentTick = NetworkTimer_v2.Instance.TickCurrent;

        // go back at least two ticks for our interp state
        if (currentTick < 3) return transform.position;

        var currTick = currentTick;

        var startBufferIndex = (currTick - 2) % k_bufferSize;
        var finishBufferIndex = (currTick - 1) % k_bufferSize;

        var start = clientStateBuffer.Get(startBufferIndex);
        var finish = clientStateBuffer.Get(finishBufferIndex);
        var fraction = NetworkTimer_v2.Instance.TickFraction;

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
        var startBufferIndex = (tick - 2) % k_bufferSize;
        var finishBufferIndex = (tick - 1) % k_bufferSize;

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
                    (int)math.ceil(ability.TeleportDistance) + 1);
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

    public float GetServerTickRate()
    {
        return NetworkTimer_v2.Instance.TickRate;
    }

    bool IsAttackCooldownFinished(Hand hand)
    {
        if (hand == Hand.Left)
        {
            var lhWearableEnum = m_playerEquipment.LeftHand.Value;
            var lhAttackEnum = m_playerAbilities.GetAttackAbilityEnum(lhWearableEnum);
            var lhAttack = m_playerAbilities.GetAbility(lhAttackEnum);
            return lhAttack != null ? lhAttack.IsCooldownFinished() : true;

        } else
        {
            var rhWearableEnum = m_playerEquipment.RightHand.Value;
            var rhAttackEnum = m_playerAbilities.GetAttackAbilityEnum(rhWearableEnum);
            var rhAttack = m_playerAbilities.GetAbility(rhAttackEnum);
            return rhAttack != null ? rhAttack.IsCooldownFinished() : true;
        }
    }

    bool IsHoldCooldownFinished(Hand hand)
    {
        if (hand == Hand.Left)
        {
            var lhWearableEnum = m_playerEquipment.LeftHand.Value;
            var lhHoldEnum = m_playerAbilities.GetHoldAbilityEnum(lhWearableEnum);
            var lhHold = m_playerAbilities.GetAbility(lhHoldEnum);
            return lhHold != null ? lhHold.IsCooldownFinished() : true;
        }
        else
        {
            var rhWearableEnum = m_playerEquipment.RightHand.Value;
            var rhHoldEnum = m_playerAbilities.GetHoldAbilityEnum(rhWearableEnum);
            var rhHold = m_playerAbilities.GetAbility(rhHoldEnum);
            return rhHold != null ? rhHold.IsCooldownFinished() : true;
        }
    }

    bool IsSpecialCooldownFinished(Hand hand)
    {
        if (hand == Hand.Left)
        {
            var lhWearableEnum = m_playerEquipment.LeftHand.Value;
            var lhSpecialEnum = m_playerAbilities.GetSpecialAbilityEnum(lhWearableEnum);
            var lhSpecial = m_playerAbilities.GetAbility(lhSpecialEnum);
            return lhSpecial != null ? lhSpecial.IsCooldownFinished() : true;
        }
        else
        {
            var rhWearableEnum = m_playerEquipment.RightHand.Value;
            var rhSpecialEnum = m_playerAbilities.GetSpecialAbilityEnum(rhWearableEnum);
            var rhSpecial = m_playerAbilities.GetAbility(rhSpecialEnum);
            return rhSpecial != null ? rhSpecial.IsCooldownFinished() : true;
        }
    }

    bool IsDashCooldownFinished()
    {
        var dash = m_playerAbilities.GetAbility(PlayerAbilityEnum.Dash);
        return dash != null ? dash.IsCooldownFinished() : true;
    }
}