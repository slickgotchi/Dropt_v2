using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(PlayerAbility), true)]
public class PlayerAbilityEditor : Editor
{
    SerializedProperty apCost;
    SerializedProperty executionDuration;
    SerializedProperty executionSlowFactor;
    SerializedProperty cooldownDuration;
    SerializedProperty cooldownSlowFactor;
    SerializedProperty isHoldAbility;
    SerializedProperty holdSlowFactor;
    SerializedProperty teleportDistance;
    SerializedProperty autoMoveDistance;
    SerializedProperty autoMoveDuration;

    private void OnEnable()
    {
        apCost = serializedObject.FindProperty("ApCost");
        executionDuration = serializedObject.FindProperty("ExecutionDuration");
        executionSlowFactor = serializedObject.FindProperty("ExecutionSlowFactor");
        cooldownDuration = serializedObject.FindProperty("CooldownDuration");
        cooldownSlowFactor = serializedObject.FindProperty("CooldownSlowFactor");
        isHoldAbility = serializedObject.FindProperty("isHoldAbility");
        holdSlowFactor = serializedObject.FindProperty("HoldSlowFactor");
        teleportDistance = serializedObject.FindProperty("TeleportDistance");
        autoMoveDistance = serializedObject.FindProperty("AutoMoveDistance");
        autoMoveDuration = serializedObject.FindProperty("AutoMoveDuration");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(apCost, new GUIContent("AP Cost", "Cost to cast this ability in AP"));

        EditorGUILayout.PropertyField(executionDuration, new GUIContent("Execution Duration", "Time (s) for the ability to run from Start() to Finish()"));
        if (executionDuration.floatValue > 0)
        {
            EditorGUILayout.PropertyField(executionSlowFactor, new GUIContent("Execution Slow Factor", "Slows player down for the AbilityDuration"));
        }

        EditorGUILayout.PropertyField(cooldownDuration, new GUIContent("Cooldown Duration", "Time (s) taken till any ability can be used after AbilityDuration is Finish()ed"));
        if (cooldownDuration.floatValue > 0)
        {
            EditorGUILayout.PropertyField(cooldownSlowFactor, new GUIContent("Cooldown Slow Factor", "Slows player down during Cooldown"));
        }

        EditorGUILayout.PropertyField(isHoldAbility, new GUIContent("Is Hold Ability", "Set this to true if the ability is a hold ability"));
        if (isHoldAbility.boolValue)
        {
            EditorGUILayout.PropertyField(holdSlowFactor, new GUIContent("Hold Slow Factor", "Slows player down during Hold period"));
        }

        EditorGUILayout.PropertyField(teleportDistance, new GUIContent("Teleport Distance", "Instant teleportation distance in the action direction at ability activation"));

        EditorGUILayout.PropertyField(autoMoveDistance, new GUIContent("Auto Move Distance", "Automatically move player over the given distance in the action direction at ability activation (Overrides SlowFactor)"));
        if (autoMoveDistance.floatValue > 0)
        {
            EditorGUILayout.PropertyField(autoMoveDuration, new GUIContent("Auto Move Duration", "Time taken to move the AutoMoveDistance. Hard capped to AbilityDuration"));
        }

        // Draw the rest of the properties
        DrawPropertiesExcluding(serializedObject, "ApCost", "ExecutionDuration", "ExecutionSlowFactor", "CooldownDuration", "CooldownSlowFactor", "isHoldAbility", "HoldSlowFactor", "TeleportDistance", "AutoMoveDistance", "AutoMoveDuration");

        serializedObject.ApplyModifiedProperties();
    }
}
