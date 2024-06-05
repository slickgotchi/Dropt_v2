using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Dropt;
using System.Collections.Generic;
using Unity.Mathematics;

public class PlayerMovementFixed : NetworkBehaviour
{
    [Header("Gotchi Stats")]
    [SerializeField] private float moveSpeed = 6.22f;

    private Vector2 m_moveVector;
    private Vector3 m_direction = new Vector3(0, -1, 0);
    private Vector3 m_velocity = new Vector3(0, -1, 0);
    private Rigidbody2D rb;

    // Netcode general
    NetworkTimer timer;
    const float k_serverTickRate = 10;
    const int k_bufferSize = 1024;

    // Netcode client
    CircularBuffer<StatePayload> clientStateBuffer;
    CircularBuffer<InputPayload> clientInputBuffer;
    StatePayload lastServerState;
    StatePayload lastProcessedState;

    // Netcode server
    CircularBuffer<StatePayload> serverStateBuffer;
    Queue<InputPayload> serverInputQueue;

    [Header("Netcode")]
    //[SerializeField] float reconciliationThreshold = 0.1f;
    [SerializeField] GameObject m_clientCircle;
    [SerializeField] GameObject m_serverCircle;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Netcode init
        timer = new NetworkTimer(k_serverTickRate);

        clientStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        clientInputBuffer = new CircularBuffer<InputPayload>(k_bufferSize);

        serverStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        serverInputQueue = new Queue<InputPayload>();
    }

    private void OnMovement(InputValue value)
    {
        if (!IsClient) return;

        m_moveVector = value.Get<Vector2>();
    }

    public override void OnNetworkSpawn()
    {
        // deparent our circles for representing server/client prediction locations
        m_clientCircle.transform.SetParent(null);
        m_serverCircle.transform.SetParent(null);
    }

    private void Update()
    {
        timer.Update(Time.deltaTime);

        // client cheat testing a fake dash
        if (IsClient)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                transform.position += m_direction * 3.5f;
            }
        }

        // Interpolation
        if (IsClient)
        {
            // go back at least two ticks for our interp state
            if (timer.CurrentTick < 3) return;

            var currTick = timer.CurrentTickAndFraction.Tick;
            var fraction = timer.CurrentTickAndFraction.Fraction;

            var startBufferIndex = currTick - 2;
            var finishBufferIndex = currTick - 1;

            var startPos = clientStateBuffer.Get(startBufferIndex).position;
            var finishPos = clientStateBuffer.Get(finishBufferIndex).position;

            transform.position = Vector3.Lerp(startPos, finishPos, fraction);
        }

        m_serverCircle.transform.position = lastServerState.position;
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
        };

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

    // this function called on SERVER
    [ServerRpc]
    void SendToServerRpc(InputPayload input)
    {
        serverInputQueue.Enqueue(input);
    }

    StatePayload ProcessMovement(InputPayload input)
    {
        var prevBufferIndex = (input.tick - 1) % k_bufferSize;

        // set starting position and velocity
        if (IsLocalPlayer) transform.position = clientStateBuffer.Get(prevBufferIndex).position;
        if (IsServer) transform.position = serverStateBuffer.Get(prevBufferIndex).position;
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
        };
    }

    // 3. set transform to whatever the latest server state is then rewind
    void HandleServerReconciliation()
    {
        if (!ShouldReconcile()) return; // get out of here if no need to reconcile

        // grab the buffer index of the last server state we've received
        int bufferIndex = lastServerState.tick % k_bufferSize;
        if (bufferIndex - 1 < 0) return;    // not enough information to reconcile

        StatePayload rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState;    // host rpcs execute immedimate, so use the previous server state in the buffer

        ReconcileState(rewindState);

        lastProcessedState = lastServerState;

        // update client predict circle
        var currBufferIndex = timer.CurrentTick % k_bufferSize;
        m_clientCircle.transform.position = clientStateBuffer.Get(currBufferIndex).position;
    }

    bool ShouldReconcile()
    {
        bool isNotDefaultServerState = !lastServerState.Equals(default);
        bool isLastStateUndefinedOrDifferent = lastProcessedState.Equals(default)
            || !lastProcessedState.Equals(lastServerState);

        return isNotDefaultServerState && isLastStateUndefinedOrDifferent;
    }

    void ReconcileState(StatePayload rewindState)
    {
        transform.position = rewindState.position;

        if (!rewindState.Equals(lastServerState)) return;

        clientStateBuffer.Add(rewindState, rewindState.tick);

        // replay all inputs from rewind state to current state
        int tickToReplay = lastServerState.tick;

        while (tickToReplay <= timer.CurrentTick)
        {
            int bufferIndex = tickToReplay % k_bufferSize;
            StatePayload statePayload = ProcessMovement(clientInputBuffer.Get(bufferIndex));
            clientStateBuffer.Add(statePayload, bufferIndex);
            tickToReplay++;
        }
    }

    void HandleServerTick()
    {
        if (!IsServer) return;

        var bufferIndex = -1;
        InputPayload inputPayload = default;
        while (serverInputQueue.Count > 0)
        {
            inputPayload = serverInputQueue.Dequeue();

            bufferIndex = inputPayload.tick % k_bufferSize;

            StatePayload statePayload = ProcessMovement(inputPayload);
            serverStateBuffer.Add(statePayload, bufferIndex);
        }

        if (bufferIndex == -1) return;

        // tell client the last state we have as a server
        SendToClientRpc(serverStateBuffer.Get(bufferIndex));
    }

    // this function called on CLIENT
    [ClientRpc]
    void SendToClientRpc(StatePayload statePayload)
    {
        lastServerState = statePayload;

        // update server circle position
        m_serverCircle.transform.position = statePayload.position;
    }

    public Vector3 GetDirection()
    {
        return m_direction;
    }

    public Vector3 GetVelocity()
    {
        return m_velocity;
    }
}
