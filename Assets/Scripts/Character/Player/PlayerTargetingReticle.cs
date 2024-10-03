using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.InputSystem;

public class PlayerTargetingReticle : MonoBehaviour
{
    public Transform ReticleTransform;

    public enum Mode
    {
        KeyboardMouse,
        Gamepad,
        KeyboardOnly,
    }
    public Mode mode = Mode.KeyboardOnly;
    public float MaxDistance = 7f;

    // cursor data
    private Vector2 m_cursorScreenPosition;
    private Vector3 m_cursorWorldPosition;

    // move data
    private Vector3 m_moveDirection;
    private Vector3 m_lastNonZeroMoveDirection = new Vector3(0,-1,0);

    // stick data
    private Vector3 m_rightStickDirection;

    //private AttackCentre m_attackCentre;
    private PlayerInput m_playerInput;
    private InputAction m_movementAction;  // Reference to the "Movement" action

    private float k_minSafeDistance = 0.01f;

    private PlayerPrediction m_playerPrediction;
    private Vector3 k_offset = new Vector3(0, 0.5f, 0);

    private void Awake()
    {
        ReticleTransform.parent = null;
        m_playerInput = GetComponent<PlayerInput>();
        m_movementAction = m_playerInput.actions["Generic_PlayerMove"];
        m_playerPrediction = GetComponent<PlayerPrediction>();

        if (Bootstrap.IsServer()) GetComponent<PlayerTargetingReticle>().enabled = false;
    }

    private void OnSignalMode_KeyboardMouse(InputValue value)
    {
        mode = Mode.KeyboardMouse;
    }

    private void OnSignalMode_Gamepad(InputValue value)
    {
        mode = Mode.Gamepad;
    }

    private void OnSignalMode_KeyboardOnly(InputValue value)
    {
        mode = Mode.KeyboardOnly;
    }

    private void OnRightStick(InputValue value)
    {
        m_rightStickDirection = value.Get<Vector2>();
    }

    private void Update()
    {
        UpdateCursorPositionData();
        UpdateMovementData();

        switch (mode)
        {
            case Mode.KeyboardMouse:
                HandleKeyboardMouse();
                break;
            case Mode.Gamepad:
                HandleGamepad();
                break;
            case Mode.KeyboardOnly:
                HandleKeyboardOnly();
                break;
            default: break;
        }
    }

    public void SetMode(Mode newMode)
    {
        mode = newMode;
    }

    public Vector3 GetActionDirection()
    {
        Vector3 actionDirection = new Vector3(0,-1,0);

        switch (mode)
        {
            case Mode.KeyboardMouse:
                var dir = m_cursorWorldPosition - (m_playerPrediction.GetLocalPlayerInterpPosition() + k_offset);
                if (dir.magnitude > k_minSafeDistance) actionDirection = dir.normalized;
                break;
            case Mode.Gamepad:
                if (m_rightStickDirection.magnitude > k_minSafeDistance) actionDirection = m_rightStickDirection.normalized;
                else actionDirection = m_lastNonZeroMoveDirection.normalized;
                break;
            case Mode.KeyboardOnly:
                actionDirection = m_lastNonZeroMoveDirection.normalized;
                break;
            default: break;
        }

        return actionDirection;
    }

    public float GetActionDistance()
    {
        float actionDistance = MaxDistance;

        switch (mode)
        {
            case Mode.KeyboardMouse:
                actionDistance = math.distance(m_playerPrediction.GetLocalPlayerInterpPosition() + k_offset, m_cursorWorldPosition);
                break;
            case Mode.Gamepad:
                if (m_rightStickDirection.magnitude > k_minSafeDistance) actionDistance = m_rightStickDirection.magnitude * MaxDistance;
                else actionDistance = MaxDistance;
                break;
            case Mode.KeyboardOnly:
                actionDistance = MaxDistance;
                break;
            default: break;
        }

        return actionDistance;
    }

    void HandleKeyboardMouse()
    {
        ReticleTransform.transform.position = m_cursorWorldPosition;
    }

    void HandleGamepad()
    {
        // Calculate the target reticle position based on the right stick input (if present)
        Vector3 targetReticlePosition;
        if (m_rightStickDirection.magnitude > k_minSafeDistance)
        {
            targetReticlePosition = m_playerPrediction.GetLocalPlayerInterpPosition() + k_offset + m_rightStickDirection * MaxDistance;
        } else
        {
            targetReticlePosition = m_playerPrediction.GetLocalPlayerInterpPosition() + k_offset + m_lastNonZeroMoveDirection.normalized * MaxDistance;
        }

        // ensure we are not offscreen
        Vector3 minBounds, maxBounds;
        GetScreenBoundsInWorld(out minBounds, out maxBounds);
        if (targetReticlePosition.y > maxBounds.y) targetReticlePosition.y = maxBounds.y;
        if (targetReticlePosition.y < minBounds.y) targetReticlePosition.y = minBounds.y;
        if (targetReticlePosition.x > maxBounds.x) targetReticlePosition.x = maxBounds.x;
        if (targetReticlePosition.x < minBounds.x) targetReticlePosition.x = minBounds.x;

        // Smoothly interpolate the reticle's position over time (adjust smoothing factor as needed)
        float smoothingFactor = 15f;  // Higher values make the reticle move faster to the target
        ReticleTransform.transform.position = Vector3.Lerp(ReticleTransform.transform.position, targetReticlePosition, Time.deltaTime * smoothingFactor);
    }

    void HandleKeyboardOnly()
    {
        // Calculate the target reticle position based on the right stick input
        Vector3 targetReticlePosition = m_playerPrediction.GetLocalPlayerInterpPosition() + k_offset + m_moveDirection.normalized * MaxDistance;

        // ensure we are not offscreen
        Vector3 minBounds, maxBounds;
        GetScreenBoundsInWorld(out minBounds, out maxBounds);
        if (targetReticlePosition.y > maxBounds.y) targetReticlePosition.y = maxBounds.y;
        if (targetReticlePosition.y < minBounds.y) targetReticlePosition.y = minBounds.y;
        if (targetReticlePosition.x > maxBounds.x) targetReticlePosition.x = maxBounds.x;
        if (targetReticlePosition.x < minBounds.x) targetReticlePosition.x = minBounds.x;

        // Smoothly interpolate the reticle's position over time (adjust smoothing factor as needed)
        float smoothingFactor = 15f;  // Higher values make the reticle move faster to the target
        ReticleTransform.transform.position = Vector3.Lerp(ReticleTransform.transform.position, targetReticlePosition, Time.deltaTime * smoothingFactor);
    }

    // update cursor position data
    void UpdateCursorPositionData()
    {
        // get mouse pos
        m_cursorScreenPosition = Input.mousePosition;

        // Convert screen position to world position
        Vector3 screenToWorldPosition = Camera.main.ScreenToWorldPoint(
            new Vector3(m_cursorScreenPosition.x, m_cursorScreenPosition.y, Camera.main.transform.position.z));

        // Since it's a 2D game, we set the Z coordinate to 0
        m_cursorWorldPosition = new Vector3(screenToWorldPosition.x, screenToWorldPosition.y, 0);
    }

    void UpdateMovementData()
    {
        m_moveDirection = m_movementAction.ReadValue<Vector2>().normalized;
        if (m_moveDirection.magnitude > 0.01f) m_lastNonZeroMoveDirection = m_moveDirection;
    }

    void GetScreenBoundsInWorld(out Vector3 minBounds, out Vector3 maxBounds)
    {
        // Get the main camera reference
        Camera cam = Camera.main;

        // Bottom-left corner of the screen in screen space (0,0)
        Vector3 bottomLeftScreen = new Vector3(0, 0, cam.transform.position.z);

        // Top-right corner of the screen in screen space (Screen.width, Screen.height)
        Vector3 topRightScreen = new Vector3(Screen.width, Screen.height, cam.transform.position.z);

        // Convert screen space to world space
        Vector3 bottomLeftWorld = cam.ScreenToWorldPoint(bottomLeftScreen);
        Vector3 topRightWorld = cam.ScreenToWorldPoint(topRightScreen);

        // Set the bounds
        minBounds = new Vector3(bottomLeftWorld.x, bottomLeftWorld.y, 0);
        maxBounds = new Vector3(topRightWorld.x, topRightWorld.y, 0);
    }

}
