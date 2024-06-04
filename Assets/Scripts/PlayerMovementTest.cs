using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Dropt;
using System.Collections.Generic;

public class PlayerMovementTest : NetworkBehaviour
{
    private Vector2 movement;
    private Rigidbody2D rb;
    [SerializeField] private float moveSpeed = 6.22f;
    private Animator animator;

    public Vector2 Direction = new Vector2(1, 0);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Netcode init

    }

    private void OnMovement(InputValue value)
    {
        movement = value.Get<Vector2>();
    }

    private void Update()
    {
    }

    private void FixedUpdate()
    {

    }

    void HandleServerTick()
    {

    }



    void HandleClientTick()
    {

    }


    void HandleServerReconciliation()
    {

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
