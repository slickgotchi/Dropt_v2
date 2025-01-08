using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Unity.Mathematics;

// INPUT HANDLING

public partial class PlayerPrediction : NetworkBehaviour
{
    // for calculating mouse positions
    private Vector2 m_cursorScreenPosition;

    private Vector2 m_holdActionDirection;

    private Vector3 m_screenToWorldPosition;

    // called every frame in the main PlayerPrediction.cs file Update()
    private void UpdateInput()
    {
        if (!Application.isFocused) return;

        // update cursor
        m_cursorScreenPosition = Input.mousePosition;

        // Convert screen position to world position
        m_screenToWorldPosition = Camera.main.ScreenToWorldPoint(
            new Vector3(m_cursorScreenPosition.x, m_cursorScreenPosition.y, Camera.main.transform.position.z));
        m_screenToWorldPosition.z = 0;
        // Since it's a 2D game, we set the Z coordinate to 0
        //m_cursorWorldPosition = new Vector3(screenToWorldPosition.x, screenToWorldPosition.y, 0);

        // handle movement
        if (IsMovementEnabled && IsInputEnabled)
        {
            m_moveDirection = m_movementAction.ReadValue<Vector2>();
            if (m_moveDirection.magnitude > 0.01f) m_lastNonZeroMoveDirection = m_moveDirection;
        }
        else
        {
            m_moveDirection = Vector3.zero;
        }

        // recalc m_holdActionDirection
        if (m_playerTargetingReticle.mode == PlayerTargetingReticle.Mode.KeyboardMouse)
        {
            var dir = (m_screenToWorldPosition - (GetLocalPlayerInterpPosition() + new Vector3(0, 0.5f, 0))).normalized;
            m_holdActionDirection = new Vector2(dir.x, dir.y);
        }
        else if (m_playerTargetingReticle.mode == PlayerTargetingReticle.Mode.Gamepad)
        {
            var dir = (m_screenToWorldPosition - (GetLocalPlayerInterpPosition() + new Vector3(0, 0.5f, 0))).normalized;
            m_holdActionDirection = new Vector2(dir.x, dir.y);
        }
        else if (m_playerTargetingReticle.mode == PlayerTargetingReticle.Mode.KeyboardOnly)
        {
            m_holdActionDirection = new Vector2(m_lastNonZeroMoveDirection.x, m_lastNonZeroMoveDirection.y);
        }
    }

    public Vector2 GetHoldActionDirection()
    {
        return m_holdActionDirection;
    }

    public float GetHoldDistanceFromPlayerAttackCentre()
    {
        var attackCentrePos = m_attackCentre.transform.position;
        return math.distance(attackCentrePos, m_screenToWorldPosition);
    }

    // Generic_PlayerMove - this is not called as we sample movement from m_movementAction every frame
    // Generic_CursorMove - this is not called as we sample the cursor every frame in UpdateInput()

    // Generic_Interact
    private void OnGeneric_Interact(InputValue value)
    {
        IsInteracting = value.isPressed;
    }

    void SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode mode)
    {
        m_playerTargetingReticle.SetMode(mode);
        m_actionDirection = m_playerTargetingReticle.GetActionDirection();
        m_actionDistance = m_playerTargetingReticle.GetActionDistance();
        m_lastNonZeroMoveDirection = m_actionDirection;
        m_actionDirectionTimer = k_actionDirectionTime;
    }

    // DASH
    private void Dash(InputValue value)
    {
        if (!IsLocalPlayer) return;
        if (!IsInputEnabled) return;

        m_triggeredAbilityEnum = PlayerAbilityEnum.Dash;
    }

    private void OnMouse_Dash(InputValue value)
    {
        Dash(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.KeyboardMouse);
    }

    private void OnKeyboard_Dash(InputValue value)
    {
        Dash(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.KeyboardOnly);
    }

    private void OnGamepad_Dash(InputValue value)
    {
        Dash(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.Gamepad);
    }

    // LEFT ATTACK
    void LeftAttack(InputValue value)
    {
        if (!IsLocalPlayer) return;
        if (!IsInputEnabled) return;

        m_abilityHand = Hand.Left;
        var lhWearable = m_playerEquipment.LeftHand.Value;
        m_triggeredAbilityEnum = m_playerAbilities.GetAttackAbilityEnum(lhWearable);
    }

    private void OnMouse_LeftAttack(InputValue value)
    {
        LeftAttack(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.KeyboardMouse);
    }

    private void OnKeyboard_LeftAttack(InputValue value)
    {
        LeftAttack(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.KeyboardOnly);
    }

    private void OnGamepad_LeftAttack(InputValue value)
    {
        LeftAttack(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.Gamepad);
    }

    // LEFT SPECIAL
    void LeftSpecial(InputValue value)
    {
        if (!IsLocalPlayer) return;
        if (!IsInputEnabled) return;

        m_abilityHand = Hand.Left;
        var lhWearable = m_playerEquipment.LeftHand.Value;
        m_triggeredAbilityEnum = m_playerAbilities.GetSpecialAbilityEnum(lhWearable);
    }

    private void OnMouse_LeftSpecial(InputValue value)
    {
        LeftSpecial(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.KeyboardMouse);
    }

    private void OnKeyboard_LeftSpecial(InputValue value)
    {
        LeftSpecial(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.KeyboardOnly);
    }

    private void OnGamepad_LeftSpecial(InputValue value)
    {
        LeftSpecial(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.Gamepad);
    }

    // LEFT HOLD START
    void LeftHoldStart(InputValue value)
    {
        if (!IsLocalPlayer) return;
        if (!IsInputEnabled) return;

        var lhWearable = m_playerEquipment.LeftHand.Value;
        m_holdStartTriggeredAbilityEnum = m_playerAbilities.GetHoldAbilityEnum(lhWearable);
        var holdAbility = m_playerAbilities.GetAbility(m_holdStartTriggeredAbilityEnum);
        m_IsShieldAbilityActive = m_holdStartTriggeredAbilityEnum == PlayerAbilityEnum.ShieldBlock;

        if (holdAbility == null) return;

        m_holdState = HoldState.LeftActive;
        m_holdChargeTime = holdAbility.HoldChargeTime;
        m_holdInputStartTick = NetworkTimer_v2.Instance.TickCurrent;
    }

    private void OnMouse_LeftHoldStart(InputValue value)
    {
        LeftHoldStart(value);
    }

    private void OnKeyboard_LeftHoldStart(InputValue value)
    {
        LeftHoldStart(value);
    }

    private void OnGamepad_LeftHoldStart(InputValue value)
    {
        LeftHoldStart(value);
    }

    // LEFT HOLD FINISH
    void LeftHoldFinish(InputValue value)
    {
        if (!IsLocalPlayer) return;
        if (!IsInputEnabled) return;
        if (m_holdState != HoldState.LeftActive) return;

        m_abilityHand = Hand.Left;
        var lhWearable = m_playerEquipment.LeftHand.Value;
        m_triggeredAbilityEnum = m_playerAbilities.GetHoldAbilityEnum(lhWearable);

        m_isHoldFinishFlag = true;

        m_holdStartTriggeredAbilityEnum = PlayerAbilityEnum.Null;
        m_holdState = HoldState.Inactive;
    }

    private void OnMouse_LeftHoldFinish(InputValue value)
    {
        LeftHoldFinish(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.KeyboardMouse);
    }

    private void OnKeyboard_LeftHoldFinish(InputValue value)
    {
        LeftHoldFinish(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.KeyboardOnly);
    }

    private void OnGamepad_LeftHoldFinish(InputValue value)
    {
        LeftHoldFinish(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.Gamepad);
    }


    // RIGHT ATTACK
    void RightAttack(InputValue value)
    {
        if (!IsLocalPlayer) return;
        if (!IsInputEnabled) return;

        m_abilityHand = Hand.Right;
        var rhWearable = m_playerEquipment.RightHand.Value;
        m_triggeredAbilityEnum = m_playerAbilities.GetAttackAbilityEnum(rhWearable);
    }

    private void OnMouse_RightAttack(InputValue value)
    {
        RightAttack(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.KeyboardMouse);
    }

    private void OnKeyboard_RightAttack(InputValue value)
    {
        RightAttack(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.KeyboardOnly);
    }

    private void OnGamepad_RightAttack(InputValue value)
    {
        RightAttack(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.Gamepad);
    }

    // RIGHT SPECIAL
    void RightSpecial(InputValue value)
    {
        if (!IsLocalPlayer) return;
        if (!IsInputEnabled) return;

        m_abilityHand = Hand.Right;
        var rhWearable = m_playerEquipment.RightHand.Value;
        m_triggeredAbilityEnum = m_playerAbilities.GetSpecialAbilityEnum(rhWearable);
    }

    private void OnMouse_RightSpecial(InputValue value)
    {
        RightSpecial(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.KeyboardMouse);
    }

    private void OnKeyboard_RightSpecial(InputValue value)
    {
        RightSpecial(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.KeyboardOnly);
    }

    private void OnGamepad_RightSpecial(InputValue value)
    {
        RightSpecial(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.Gamepad);
    }

    // RIGHT HOLD START
    void RightHoldStart(InputValue value)
    {
        if (!IsLocalPlayer) return;
        if (!IsInputEnabled) return;

        var rhWearable = m_playerEquipment.RightHand.Value;
        m_holdStartTriggeredAbilityEnum = m_playerAbilities.GetHoldAbilityEnum(rhWearable);
        var holdAbility = m_playerAbilities.GetAbility(m_holdStartTriggeredAbilityEnum);
        m_IsShieldAbilityActive = m_holdStartTriggeredAbilityEnum == PlayerAbilityEnum.ShieldBlock;

        if (holdAbility == null) return;

        m_holdChargeTime = holdAbility.HoldChargeTime;
        m_holdState = HoldState.RightActive;
        m_holdInputStartTick = NetworkTimer_v2.Instance.TickCurrent;
    }

    private void OnMouse_RightHoldStart(InputValue value)
    {
        RightHoldStart(value);
    }

    private void OnKeyboard_RightHoldStart(InputValue value)
    {
        RightHoldStart(value);
    }

    private void OnGamepad_RightHoldStart(InputValue value)
    {
        RightHoldStart(value);
    }

    // RIGHT HOLD FINISH
    void RightHoldFinish(InputValue value)
    {
        if (!IsLocalPlayer) return;
        if (!IsInputEnabled) return;
        if (m_holdState != HoldState.RightActive) return;

        m_abilityHand = Hand.Right;
        var rhWearable = m_playerEquipment.RightHand.Value;
        m_triggeredAbilityEnum = m_playerAbilities.GetHoldAbilityEnum(rhWearable);

        m_isHoldFinishFlag = true;

        m_holdStartTriggeredAbilityEnum = PlayerAbilityEnum.Null;
        m_holdState = HoldState.Inactive;
    }

    private void OnMouse_RightHoldFinish(InputValue value)
    {
        RightHoldFinish(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.KeyboardMouse);
    }

    private void OnKeyboard_RightHoldFinish(InputValue value)
    {
        RightHoldFinish(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.KeyboardOnly);
    }

    private void OnGamepad_RightHoldFinish(InputValue value)
    {
        RightHoldFinish(value);
        SetupActionDirectionDistanceAndMove(PlayerTargetingReticle.Mode.Gamepad);
    }
}
