using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NetworkObjectSpawner))]
public class NetworkObjectSpawnerEditor : Editor
{
    SerializedProperty typeProp;
    SerializedProperty elapsedTimeProp;
    SerializedProperty otherSpawnerProp;
    SerializedProperty noSpawnChanceProp;
    SerializedProperty spawnEnemiesProp;
    SerializedProperty spawnDestructiblesProp;
    SerializedProperty spawnInteractablesProp;
    SerializedProperty spawnPropsProp;

    private void OnEnable()
    {
        typeProp = serializedObject.FindProperty("activationType");
        elapsedTimeProp = serializedObject.FindProperty("activateOnElapsedTime");
        otherSpawnerProp = serializedObject.FindProperty("activateOnOtherSpawnerCleared");
        noSpawnChanceProp = serializedObject.FindProperty("NoSpawnChance");
        spawnEnemiesProp = serializedObject.FindProperty("SpawnEnemies");
        spawnDestructiblesProp = serializedObject.FindProperty("SpawnDestructibles");
        spawnInteractablesProp = serializedObject.FindProperty("SpawnInteractables");
        spawnPropsProp = serializedObject.FindProperty("SpawnProps");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(typeProp);

        NetworkObjectSpawner.ActivationType type = (NetworkObjectSpawner.ActivationType)typeProp.enumValueIndex;
        if (type == NetworkObjectSpawner.ActivationType.ElapsedTime)
        {
            EditorGUILayout.PropertyField(elapsedTimeProp);
        }
        else if (type == NetworkObjectSpawner.ActivationType.OtherSpawnerCleared)
        {
            EditorGUILayout.PropertyField(otherSpawnerProp);
        }

        EditorGUILayout.PropertyField(noSpawnChanceProp);
        EditorGUILayout.PropertyField(spawnEnemiesProp, true);
        EditorGUILayout.PropertyField(spawnDestructiblesProp, true);
        EditorGUILayout.PropertyField(spawnInteractablesProp, true);
        EditorGUILayout.PropertyField(spawnPropsProp, true);

        serializedObject.ApplyModifiedProperties();
    }
}
