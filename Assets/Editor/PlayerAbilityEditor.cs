using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(PlayerAbility), true)]
public class PlayerAbilityEditor : Editor
{
    SerializedProperty abilityType;
    //SerializedProperty isSpecialAbility;
    SerializedProperty apCost;
    SerializedProperty damageMultiplier;
    SerializedProperty executionDuration;
    SerializedProperty executionSlowFactor;
    SerializedProperty cooldownDuration;
    SerializedProperty cooldownSlowFactor;
    //SerializedProperty isHoldAbility;
    SerializedProperty holdSlowFactor;
    SerializedProperty holdChargeTime;
    SerializedProperty teleportDistance;
    SerializedProperty autoMoveDistance;
    SerializedProperty autoMoveDuration;
    SerializedProperty knockbackDistance;
    SerializedProperty knockbackStunDuration;
    SerializedProperty playerAbilityCentreOffset;
    SerializedProperty audioOnActivate;

    private void OnEnable()
    {
        abilityType = serializedObject.FindProperty("abilityType");
        //isSpecialAbility = serializedObject.FindProperty("IsSpecialAbility");
        apCost = serializedObject.FindProperty("ApCost");
        damageMultiplier = serializedObject.FindProperty("DamageMultiplier");
        executionDuration = serializedObject.FindProperty("ExecutionDuration");
        executionSlowFactor = serializedObject.FindProperty("ExecutionSlowFactor");
        cooldownDuration = serializedObject.FindProperty("CooldownDuration");
        cooldownSlowFactor = serializedObject.FindProperty("CooldownSlowFactor");
        //isHoldAbility = serializedObject.FindProperty("isHoldAbility");
        holdSlowFactor = serializedObject.FindProperty("HoldSlowFactor");
        holdChargeTime = serializedObject.FindProperty("HoldChargeTime");
        teleportDistance = serializedObject.FindProperty("TeleportDistance");
        autoMoveDistance = serializedObject.FindProperty("AutoMoveDistance");
        autoMoveDuration = serializedObject.FindProperty("AutoMoveDuration");
        knockbackDistance = serializedObject.FindProperty("KnockbackDistance");
        knockbackStunDuration = serializedObject.FindProperty("KnockbackStunDuration");
        playerAbilityCentreOffset = serializedObject.FindProperty("PlayerAbilityCentreOffset");
        audioOnActivate = serializedObject.FindProperty("audioOnActivate");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //EditorGUILayout.PropertyField(isSpecialAbility, new GUIContent("Is Special Ability", "Set to true if this ability should use Special AP Cost from wearable-data spreadsheet"));

        EditorGUILayout.LabelField("General", EditorStyles.boldLabel);

        // ability type
        EditorGUILayout.PropertyField(abilityType, new GUIContent("Ability Type", "Set the type of ability"));

        // ap cost
        //if ((PlayerAbility.AbilityType)abilityType.enumValueIndex == PlayerAbility.AbilityType.Special)
        {
            EditorGUILayout.PropertyField(apCost, new GUIContent("AP Cost", "Cost to cast this ability in AP"));
        }

        // damage multiplier
        EditorGUILayout.PropertyField(damageMultiplier, new GUIContent("Damage Multiplier", "Damage multiplier to apply to this type of ability"));

        EditorGUILayout.LabelField("Timings", EditorStyles.boldLabel);

        // execution duration
        EditorGUILayout.PropertyField(executionDuration, new GUIContent("Execution Duration", "Time (s) for the ability to run from Start() to Finish()"));
        if (executionDuration.floatValue > 0)
        {
            EditorGUILayout.PropertyField(executionSlowFactor, new GUIContent("Execution Slow Factor", "Slows player down for the AbilityDuration"));
        }

        // cooldown duration
        EditorGUILayout.PropertyField(cooldownDuration, new GUIContent("Cooldown Duration", "Time (s) taken till any ability can be used after AbilityDuration is Finish()ed"));

        // cooldown slow factor
        if (cooldownDuration.floatValue > 0)
        {
            //EditorGUILayout.PropertyField(cooldownSlowFactor, new GUIContent("Cooldown Slow Factor", "Slows player down during Cooldown"));
        }

        // hold parameters
        if ((PlayerAbility.AbilityType)abilityType.enumValueIndex == PlayerAbility.AbilityType.Hold)
        {
            EditorGUILayout.PropertyField(holdSlowFactor, new GUIContent("Hold Slow Factor", "Slows player down during Hold period"));
            EditorGUILayout.PropertyField(holdChargeTime, new GUIContent("Hold Charge Time", "Time taken to fully charge hold ability"));
        }

        EditorGUILayout.LabelField("Movement (Optional)", EditorStyles.boldLabel);

        // teleport distance
        EditorGUILayout.PropertyField(teleportDistance, new GUIContent("Teleport Distance", "Instant teleportation distance in the action direction at ability activation"));

        // auto move distance
        EditorGUILayout.PropertyField(autoMoveDistance, new GUIContent("Auto Move Distance", "Automatically move player over the given distance in the action direction at ability activation (Overrides SlowFactor)"));

        // auto move duration
        if (autoMoveDistance.floatValue > 0)
        {
            EditorGUILayout.PropertyField(autoMoveDuration, new GUIContent("Auto Move Duration", "Time taken to move the AutoMoveDistance. Hard capped to AbilityDuration"));
        }

        EditorGUILayout.LabelField("Knockback", EditorStyles.boldLabel);

        // knock bakc
        EditorGUILayout.PropertyField(knockbackDistance, new GUIContent("Knockback Distance", "Distance enemies are knocked back"));
        EditorGUILayout.PropertyField(knockbackStunDuration, new GUIContent("Knockback Stun Duration", "Time an enemy is stunned after a knockback"));

        EditorGUILayout.LabelField("Ability Offset", EditorStyles.boldLabel);

        // player ability centre offset
        EditorGUILayout.PropertyField(playerAbilityCentreOffset, new GUIContent("Player Ability Centre Offset", "How far away from the players centre this ability activates at"));

        // audio on activate
        EditorGUILayout.PropertyField(audioOnActivate, new GUIContent("Audio On Activate", "Audio played when ability is activated"));

        // Draw the rest of the properties
        DrawPropertiesExcluding(serializedObject, "DamageMultiplier", "PlayerAbilityCentreOffset", "m_Script", "ApCost", "ExecutionDuration", "ExecutionSlowFactor", "abilityType",
            "CooldownDuration", "CooldownSlowFactor", "isHoldAbility", "HoldSlowFactor", "HoldChargeTime", "TeleportDistance", "AutoMoveDistance", "AutoMoveDuration", "IsSpecialAbility",
            "KnockbackDistance", "KnockbackStunDuration", "audioOnActivate");

        serializedObject.ApplyModifiedProperties();
    }
}
