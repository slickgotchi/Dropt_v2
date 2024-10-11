using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Spawner_SpawnCondition))]
public class Spawner_SpawnConditionEditor : Editor
{
    SerializedProperty spawnCondition;
    SerializedProperty elapsedTime;
    SerializedProperty destroyAllWithSpawnerId;
    SerializedProperty touchTriggerWithSpawnerId;
    SerializedProperty spawnTimeAfterTrigger;

    private void OnEnable()
    {
        // Link the SerializedProperties to the corresponding fields in your script
        spawnCondition = serializedObject.FindProperty("spawnCondition");
        elapsedTime = serializedObject.FindProperty("elapsedTime");
        destroyAllWithSpawnerId = serializedObject.FindProperty("destroyAllWithSpawnerId");
        touchTriggerWithSpawnerId = serializedObject.FindProperty("touchTriggerWithSpawnerId");
        spawnTimeAfterTrigger = serializedObject.FindProperty("spawnTimeAfterTrigger");
    }

    public override void OnInspectorGUI()
    {
        // Update the serialized object
        serializedObject.Update();

        // Draw the common properties
        EditorGUILayout.PropertyField(spawnCondition);

        // Show/hide fields based on the selected spawnCondition
        switch ((Level.LevelSpawn.SpawnCondition)spawnCondition.enumValueIndex)
        {
            case Level.LevelSpawn.SpawnCondition.ElapsedTime:
                EditorGUILayout.PropertyField(elapsedTime);
                break;
            case Level.LevelSpawn.SpawnCondition.PlayerDestroyAllWithSpawnerId:
                EditorGUILayout.PropertyField(destroyAllWithSpawnerId);
                break;
            case Level.LevelSpawn.SpawnCondition.PlayerTouchTriggerWithSpawnerId:
                EditorGUILayout.PropertyField(touchTriggerWithSpawnerId);
                EditorGUILayout.PropertyField(spawnTimeAfterTrigger);
                break;
        }

        // Apply any changes made in the inspector
        serializedObject.ApplyModifiedProperties();
    }
}
