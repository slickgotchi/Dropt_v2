using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Dropt;
using System.Collections.Generic;
using Unity.Mathematics;

public class PlayerMovementSlick : NetworkBehaviour
{
    [Header("Gotchi Stats")]
    [SerializeField] private float moveSpeed = 6.22f;

    private Vector2 m_moveVector;
    private Vector3 m_direction = new Vector3(0, -1, 0);
    private Rigidbody2D rb;

    // Netcode general
    NetworkTimer timer;
    const float k_serverTickRate = 50;
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
    [SerializeField] float reconciliationThreshold = 0.1f;
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

        //if (IsServer)
        //{
        //    Application.targetFrameRate = 60;
        //}
    }

    private void OnMovement(InputValue value)
    {
        m_moveVector = value.Get<Vector2>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
        }
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
        if (!IsClient) return;

        var currentTick = timer.CurrentTick;
        var bufferIndex = currentTick % k_bufferSize;   // this just ensures we go back to index 0 when tick goes past buffer size

        // assemble input
        InputPayload inputPayload = new InputPayload()
        {
            tick = currentTick,
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
        Move(input.moveDirection);

        return new StatePayload
        {
            tick = input.tick,
            position = transform.position,
            velocity = rb.velocity,
        };
    }

    void Move(Vector2 inputVector)
    {
        // WARNING: THIS MIGHT BE WHERE STUFF IS GOING WRONG
        rb.velocity = inputVector * moveSpeed;

        if (inputVector.x != 0 || inputVector.y != 0) m_direction = inputVector;
    }

    

    // 3. set transform to whatever the latest server state is then rewind
    void HandleServerReconciliation()
    {
        if (!ShouldReconcile()) return; // get out of here if no need to reconcile

        // grab the buffer index of the last server state we've received
        int bufferIndex = lastServerState.tick % k_bufferSize;
        if (bufferIndex - 1 < 0) return;    // not enough information to reconcile

        StatePayload rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState;    // host rpcs execute immedimate, so use the previous server state in the buffer
        StatePayload clientState = clientStateBuffer.Get(bufferIndex);

        m_serverCircle.transform.position = rewindState.position;
        m_clientCircle.transform.position = clientState.position;
        
        float positionError = Vector3.Distance(rewindState.position, clientState.position);
        if (positionError > reconciliationThreshold)
        {
            //Debug.Log("need to reconcile!");
            ReconcileState(rewindState);
        }

        lastProcessedState = lastServerState;
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
        rb.velocity = rewindState.velocity;

        if (!rewindState.Equals(lastServerState)) return;

        clientStateBuffer.Add(rewindState, rewindState.tick);

        // replay all inputs from rewind state to current state
        int tickToReplay = lastServerState.tick;

        while (tickToReplay < timer.CurrentTick)
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
        if (!IsOwner) return;
        lastServerState = statePayload;

        m_serverCircle.transform.position = statePayload.position;
    }

    



    public Vector3 GetDirection()
    {
        return m_direction;
    }
}
