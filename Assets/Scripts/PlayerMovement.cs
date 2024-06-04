using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Dropt;
using System.Collections.Generic;

public class PlayerMovement : NetworkBehaviour
{
    private Vector2 movement;
    private Rigidbody2D rb;
    [SerializeField] private float moveSpeed = 6.22f;
    private Animator animator;

    public Vector2 Direction = new Vector2(1, 0);

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
    [SerializeField] float reconciliationThreshold = 10f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Netcode init
        timer = new NetworkTimer(k_serverTickRate);

        clientStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        clientInputBuffer = new CircularBuffer<InputPayload>(k_bufferSize);

        serverStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        serverInputQueue = new Queue<InputPayload>();
    }

    private void OnMovement(InputValue value)
    {
        movement = value.Get<Vector2>();
    }

    private void Update()
    {
        timer.Update(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        while (timer.ShouldTick())
        {
            HandleClientTick();
            HandleServerTick();
        }
    }

    void HandleServerTick()
    {
        var bufferIndex = -1;
        while (serverInputQueue.Count > 0)
        {
            InputPayload inputPayload = serverInputQueue.Dequeue();

            bufferIndex = inputPayload.tick % k_bufferSize;

            StatePayload statePayload = SimulateMovement(inputPayload);
            serverStateBuffer.Add(statePayload, bufferIndex);
        }

        if (bufferIndex == -1) return;
        SendToClientRpc(serverStateBuffer.Get(bufferIndex));
    }

    StatePayload SimulateMovement(InputPayload inputPayload)
    {
        Physics.simulationMode = SimulationMode.Script;

        Move(inputPayload.inputVector);
        Physics.Simulate(Time.fixedDeltaTime);

        Physics.simulationMode = SimulationMode.FixedUpdate;

        return new StatePayload()
        {
            tick = inputPayload.tick,
            position = transform.position,
            velocity = rb.velocity,
        };
    }

    [ClientRpc]
    void SendToClientRpc(StatePayload statePayload)
    {
        if (!IsOwner) return;
        lastServerState = statePayload;
    }

    void HandleClientTick()
    {
        if (!IsClient) return;

        var currentTick = timer.CurrentTick;
        var bufferIndex = currentTick % k_bufferSize;

        InputPayload inputPayload = new InputPayload()
        {
            tick = currentTick,
            inputVector = movement,
        };

        clientInputBuffer.Add(inputPayload, bufferIndex);
        SendToServerRpc(inputPayload);

        StatePayload statePayload = ProcessMovement(inputPayload);
        clientStateBuffer.Add(statePayload, bufferIndex);

        HandleServerReconciliation();
    }

    bool ShouldReconcile()
    {
        bool isNewServerState = !lastServerState.Equals(default);
        bool isLastStateUndefinedOrDifferent = lastProcessedState.Equals(default) 
            || !lastProcessedState.Equals(lastServerState);

        return isNewServerState && isLastStateUndefinedOrDifferent;
    }

    void HandleServerReconciliation()
    {
        if (!ShouldReconcile()) return;

        float positionError;
        int bufferIndex;
        StatePayload rewindState = default;

        bufferIndex = lastServerState.tick % k_bufferSize;
        if (bufferIndex - 1 < 0) return;    // not enough information to reconcile

        rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState;    // host rpcs execute immedimate, so use last server state
        positionError = Vector3.Distance(rewindState.position, clientStateBuffer.Get(bufferIndex).position);

        if (positionError > reconciliationThreshold)
        {
            ReconcileState(rewindState);
        }

        lastProcessedState = lastServerState;
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

    [ServerRpc]
    void SendToServerRpc(InputPayload input)
    {
        serverInputQueue.Enqueue(input);
    }

    StatePayload ProcessMovement(InputPayload input)
    {
        Move(input.inputVector);

        return new StatePayload()
        {
            tick = input.tick,
            position = transform.position,
            velocity = rb.velocity,
        };
    }

    void Move(Vector3 inputVector)
    {
        rb.velocity = inputVector * moveSpeed;

        if (inputVector.x != 0 || inputVector.y != 0)
        {
            Direction = inputVector;
            animator.Play("Player_Move");
        }
        else
        {
            animator.Play("Player_Idle");
        }
    }


}
