using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayerInput : MonoBehaviour
{
    private void OnMouse_LeftAttack(InputValue value)
    {
        Debug.Log("Left attack");
    }
}
