using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class LocalVelocity : MonoBehaviour
{
    public Vector3 Value;
    public bool IsMoving;
    public Vector3 LastNonZeroVelocity;

    private Vector3 m_prevPosition = Vector3.zero;
    private float k_tolerance = 0.1f;

    // Update is called once per frame
    void Update()
    {
        var delta = transform.position - m_prevPosition;
        m_prevPosition = transform.position;

        Value = delta / Time.deltaTime;

        if (math.abs(Value.x) < k_tolerance && math.abs(Value.y) < k_tolerance)
        {
            Value = Vector3.zero;
            IsMoving = false;
        } else
        {
            IsMoving = true;
            LastNonZeroVelocity = Value;
        }
    }
}
