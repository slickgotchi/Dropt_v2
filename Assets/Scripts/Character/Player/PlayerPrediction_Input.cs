using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Unity.Mathematics;

// INPUT HANDLING

public partial class PlayerPrediction : NetworkBehaviour
{
    // called every frame in the main PlayerPrediction.cs file Update()
    private void UpdateInput()
    {
        // update cursor
        m_cursorScreenPosition = Input.mousePosition;

        // Convert screen position to world position
        Vector3 screenToWorldPosition = Camera.main.ScreenToWorldPoint(
            new Vector3(m_cursorScreenPosition.x, m_cursorScreenPosition.y, Camera.main.transform.position.z));

        // Since it's a 2D game, we set the Z coordinate to 0
        m_cursorWorldPosition = new Vector3(screenToWorldPosition.x, screenToWorldPosition.y, 0);

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
        var lhWearable = GetComponent<PlayerEquipment>().LeftHand.Value;
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
        var lhWearable = GetComponent<PlayerEquipment>().LeftHand.Value;
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

        var lhWearable = GetComponent<PlayerEquipment>().LeftHand.Value;
        m_holdStartTriggeredAbilityEnum = m_playerAbilities.GetHoldAbilityEnum(lhWearable);
        var holdAbility = m_playerAbilities.GetAbility(m_holdStartTriggeredAbilityEnum);
        m_IsShieldAbilityActive = m_holdStartTriggeredAbilityEnum == PlayerAbilityEnum.ShieldBlock;

        if (holdAbility == null) return;

        m_holdState = HoldState.LeftActive;
        m_holdChargeTime = holdAbility.HoldChargeTime;
        m_holdInputStartTick = timer.CurrentTick;
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
        var lhWearable = GetComponent<PlayerEquipment>().LeftHand.Value;
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
        var rhWearable = GetComponent<PlayerEquipment>().RightHand.Value;
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
        var rhWearable = GetComponent<PlayerEquipment>().RightHand.Value;
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

        var rhWearable = GetComponent<PlayerEquipment>().RightHand.Value;
        m_holdStartTriggeredAbilityEnum = m_playerAbilities.GetHoldAbilityEnum(rhWearable);
        var holdAbility = m_playerAbilities.GetAbility(m_holdStartTriggeredAbilityEnum);
        m_IsShieldAbilityActive = m_holdStartTriggeredAbilityEnum == PlayerAbilityEnum.ShieldBlock;

        if (holdAbility == null) return;

        m_holdChargeTime = holdAbility.HoldChargeTime;
        m_holdState = HoldState.RightActive;
        m_holdInputStartTick = timer.CurrentTick;
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
        var rhWearable = GetComponent<PlayerEquipment>().RightHand.Value;
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
