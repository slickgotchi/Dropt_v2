using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Dropt;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;

public class PlayerMovementAndDash : NetworkBehaviour
{
    [Header("Gotchi Stats")]
    [SerializeField] private float moveSpeed = 6.22f;

    private Vector3 m_moveVector;
    private Vector3 m_direction = new Vector3(0, -1, 0);
    private Vector3 m_velocity = new Vector3(0, -1, 0);
    private Vector3 m_actionDirection = new Vector3(0, -1, 0);
    private float m_actionDirectionTimer = 0;
    private float k_actionDirectionTime = 0.5f;
    private bool m_isDash = false;

    private Rigidbody2D rb;

    // Netcode general
    NetworkTimer timer;
    const float k_serverTickRate = 10;
    const int k_bufferSize = 128;

    // Netcode client
    CircularBuffer<StatePayload> clientStateBuffer;
    CircularBuffer<InputPayload> clientInputBuffer;
    StatePayload lastServerState;

    List<StatePayload> lastServerStateArray;

    // Netcode server
    CircularBuffer<StatePayload> serverStateBuffer;
    Queue<InputPayload> serverInputQueue;


    [Header("Netcode")]
    [SerializeField] GameObject m_clientCircle;
    [SerializeField] GameObject m_serverCircle;

    [Header("Utility")]
    [SerializeField] Camera playerCamera;

    private Vector2 m_cursorScreenPosition;
    private Vector3 m_cursorWorldPosition;

    // to keep track of our tick delta to the server
    private int m_remoteClientTickDelta = 0;
    List<int> m_remoteClientTickDeltas = new List<int>();

    private bool m_isDashAnimPlayed = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnMovement(InputValue value)
    {
        if (!IsLocalPlayer) return;

        m_moveVector = value.Get<Vector2>();
    }

    private void OnCursor_Dash(InputValue value)
    {
        if (!IsLocalPlayer) return;
        m_actionDirection = math.normalizesafe(m_cursorWorldPosition - transform.position);
        m_actionDirectionTimer = k_actionDirectionTime;
        m_direction = m_actionDirection;
        m_isDash = true;
    }

    private void OnKeys_Dash(InputValue value)
    {
        if (!IsLocalPlayer) return;
        m_actionDirection = m_direction;
        m_actionDirectionTimer = k_actionDirectionTime;
        m_isDash = true;
    }

    private void OnMousePosition(InputValue value)
    {
        if (!IsLocalPlayer) return;
        // Get the screen position from the action
        m_cursorScreenPosition = value.Get<Vector2>();
    }

    private void UpdateCursorWorldPosition()
    {
        // Convert screen position to world position
        Vector3 screenToWorldPosition = playerCamera.ScreenToWorldPoint(
            new Vector3(m_cursorScreenPosition.x, m_cursorScreenPosition.y, Camera.main.transform.position.z));

        // Since it's a 2D game, we set the Z coordinate to 0
        m_cursorWorldPosition = new Vector3(screenToWorldPosition.x, screenToWorldPosition.y, 0);
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

        lastServerStateArray = new List<StatePayload>();
    }

    private void Update()
    {
        timer.Update(Time.deltaTime);

        m_actionDirectionTimer -= Time.deltaTime;

        if (IsLocalPlayer || IsHost)
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

        // assemble input
        InputPayload inputPayload = new InputPayload()
        {
            tick = currentTick,
            networkObjectId = NetworkObjectId,
            moveDirection = m_moveVector,
            actionDirection = m_actionDirection,
            isDash = m_isDash,
        };

        // reset any input booleans
        m_isDash = false;

        // send input to server
        SendToServerRpc(inputPayload);

        // store in client input buffer
        clientInputBuffer.Add(inputPayload, bufferIndex);

        // locally process the movement and save our new state for this current tick
        StatePayload statePayload = ProcessMovement(inputPayload); // not a script simulation, use default fixed update
        clientStateBuffer.Add(statePayload, bufferIndex);

        // do server reconciliation
        HandleServerReconciliation();
    }

    // 3. set transform to whatever the latest server state is then rewind
    void HandleServerReconciliation()
    {
        // grab the buffer index of the last server state we've received
        int bufferIndex = lastServerState.tick % k_bufferSize;
        if (bufferIndex - 1 < 0) return;    // not enough information to reconcile

        StatePayload rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState;    // host rpcs execute immedimate, so use the previous server state in the buffer

        ReconcileState(rewindState);
    }

    void ReconcileState(StatePayload rewindState)
    {
        transform.position = rewindState.position;

        clientStateBuffer.Add(rewindState, rewindState.tick);

        // replay all inputs from rewind state to current state
        int tickToReplay = lastServerState.tick + 1;

        while (tickToReplay <= timer.CurrentTick)
        {
            int bufferIndex = tickToReplay % k_bufferSize;
            StatePayload statePayload = ProcessMovement(clientInputBuffer.Get(bufferIndex), true);

            clientStateBuffer.Add(statePayload, bufferIndex);
            tickToReplay++;
        }
    }

    StatePayload ProcessMovement(InputPayload input, bool isReconciliation = false)
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

        // check for dash
        if (input.isDash)
        {
            transform.position += input.actionDirection * 3.5f;
        }

        // set velocity
        rb.velocity = input.moveDirection * moveSpeed;

        Physics2D.simulationMode = SimulationMode2D.Script;

        // simulate
        var simulationTime = 1f / k_serverTickRate;
        while (simulationTime > 0f)
        {
            Physics2D.Simulate(Time.fixedDeltaTime);    // need to simulate at fixedDeltaTime
            simulationTime -= Time.fixedDeltaTime;
        }
        rb.velocity = Vector3.zero;

        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;

        // update private variables
        if (input.moveDirection.x != 0 || input.moveDirection.y != 0) m_direction = input.moveDirection;
        m_velocity = input.moveDirection * moveSpeed;

        return new StatePayload
        {
            tick = input.tick,
            networkObjectId = input.networkObjectId,
            position = transform.position,
            isDash = input.isDash,
        };
    }

    void HandleServerTick()
    {
        if (!IsServer) return;

        var bufferIndex = -1;
        InputPayload inputPayload = default;
        StatePayload statePayload = default;

        while (serverInputQueue.Count > 0)
        {
            inputPayload = serverInputQueue.Dequeue();

            bufferIndex = inputPayload.tick % k_bufferSize;

            statePayload = ProcessMovement(inputPayload);
            serverStateBuffer.Add(statePayload, bufferIndex);

            // tell client the last state we have as a server
            SendToClientRpc(statePayload);
        }
    }

    // this function executed on CLIENT
    [ClientRpc]
    void SendToClientRpc(StatePayload statePayload)
    {
        // save last server state
        lastServerState = statePayload;

        // append state to last server state array
        lastServerStateArray.Add(statePayload);
        if (lastServerStateArray.Count > 5) lastServerStateArray.RemoveAt(0);

        // update tick delta (if required)
        //m_remoteClientTickDelta = timer.CurrentTick - statePayload.tick;

        // get average delta tick
        var deltaTick = timer.CurrentTick - statePayload.tick;
        m_remoteClientTickDeltas.Add(deltaTick);
        if (m_remoteClientTickDeltas.Count > 10) m_remoteClientTickDeltas.RemoveAt(0);
        float sum = 0;
        foreach (var delta in m_remoteClientTickDeltas) sum += delta;
        m_remoteClientTickDelta = (int)math.round(sum/m_remoteClientTickDeltas.Count);
    }

    // this function executed on SERVER
    [Rpc(SendTo.Server)]
    void SendToServerRpc(InputPayload input)
    {
        serverInputQueue.Enqueue(input);
    }

    private Vector3 GetRemotePlayerInterpPosition()
    {
        if (lastServerStateArray.Count < 5) return transform.position;

        var targetTick = timer.CurrentTickAndFraction.Tick - m_remoteClientTickDelta - 3;

        // find out where we are in last server state array
        int a = 0;
        int b = 0;

        for (int i = 0; i < lastServerStateArray.Count - 1; i++)
        {
            if (targetTick == lastServerStateArray[i].tick)
            {
                a = i;
                b = i + 1;
                break;
            }
        }

        // something went wrong so just return original position
        if (a == 0 && b == 0) return transform.position;

        // store interp values
        var fraction = timer.CurrentTickAndFraction.Fraction;
        var start = lastServerStateArray[a];
        var finish = lastServerStateArray[b];

        // Draw dash shadow if we dashed
        IfDashInputDrawShadow(start, finish);

        // otherwise return our lerp'd position
        if (finish.isDash) return finish.position;
        else return Vector3.Lerp(start.position, finish.position, fraction);
    }

    private Vector3 GetLocalPlayerInterpPosition()
    {
        // go back at least two ticks for our interp state
        if (timer.CurrentTick < 3) return transform.position;

        var currTick = timer.CurrentTickAndFraction.Tick;
        var fraction = timer.CurrentTickAndFraction.Fraction;

        var startBufferIndex = (currTick - 2) % k_bufferSize;
        var finishBufferIndex = (currTick - 1) % k_bufferSize;

        var start = clientStateBuffer.Get(startBufferIndex);
        var finish = clientStateBuffer.Get(finishBufferIndex);

        // Draw dash shadow if we dashed
        IfDashInputDrawShadow(start, finish);

        // otherwise return our lerp'd position
        if (finish.isDash) return finish.position; 
        else return Vector3.Lerp(start.position, finish.position, fraction);
    }

    void IfDashInputDrawShadow(StatePayload start, StatePayload finish)
    {
        if (finish.isDash)
        {
            // play dash anim
            if (!m_isDashAnimPlayed)
            {
                gameObject.GetComponentInChildren<DashTrailSpawner>().DrawShadow(start.position, finish.position, 4);
                m_isDashAnimPlayed = true;
            }
        }
        else
        {
            m_isDashAnimPlayed = false;
        }
    }

    public Vector3 GetDirection()
    {
        return m_direction;
    }

    public Vector3 GetVelocity()
    {
        return m_velocity;
    }

    public Vector3 GetFacingDirection()
    {
        if (m_actionDirectionTimer > 0)
        {
            return math.normalize(m_actionDirection);
        }
        else
        {
            return m_direction;
        }
    }
}
