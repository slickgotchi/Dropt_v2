using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(PlayerAbility), true)]
public class PlayerAbilityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PlayerAbility ability = (PlayerAbility)target;

        EditorGUILayout.LabelField("Base Ability Parameters", EditorStyles.boldLabel);

        ability.ApCost = EditorGUILayout.IntField(new GUIContent("AP Cost", "Cost to cast this ability in AP"), ability.ApCost);

        ability.ExecutionDuration = EditorGUILayout.FloatField(new GUIContent("Execution Duration", "Time (s) for the ability to run from Start() to Finish()"), ability.ExecutionDuration);
        if (ability.ExecutionDuration > 0)
        {
            ability.ExecutionSlowFactor = EditorGUILayout.FloatField(new GUIContent("Execution Slow Factor", "Slows player down for the AbilityDuration"), ability.ExecutionSlowFactor);
        }

        ability.CooldownDuration = EditorGUILayout.FloatField(new GUIContent("Cooldown Duration", "Time (s) taken till any ability can be used after AbilityDuration is Finish()ed"), ability.CooldownDuration);
        if (ability.CooldownDuration > 0)
        {
            ability.CooldownSlowFactor = EditorGUILayout.FloatField(new GUIContent("Cooldown Slow Factor", "Slows player down during Cooldown"), ability.CooldownSlowFactor);
        }

        ability.isHoldAbility = EditorGUILayout.Toggle(new GUIContent("Is Hold Ability", "Set this to true if the ability is a hold ability"), ability.isHoldAbility);
        if (ability.isHoldAbility)
        {
            ability.HoldSlowFactor = EditorGUILayout.FloatField(new GUIContent("Hold Slow Factor", "Slows player down during Hold period"), ability.HoldSlowFactor);
        }

        ability.TeleportDistance = EditorGUILayout.FloatField(new GUIContent("Teleport Distance", "Instant teleportation distance in the action direction at ability activation"), ability.TeleportDistance);
        ability.AutoMoveDistance = EditorGUILayout.FloatField(new GUIContent("Auto Move Distance", "Automatically move player over the given distance in the action direction at ability activation (Overrides SlowFactor)"), ability.AutoMoveDistance);
        if (ability.AutoMoveDistance > 0)
        {
            ability.AutoMoveDuration = EditorGUILayout.FloatField(new GUIContent("Auto Move Duration", "Time taken to move the AutoMoveDistance. Hard capped to AbilityDuration"), ability.AutoMoveDuration);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(ability);
        }
    }
}
