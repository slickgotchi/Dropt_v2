//using UnityEngine;
//using UnityEditor;

//[CustomEditor(typeof(Spawner_NetworkObject_v2))]
//public class Spawner_NetworkObject_v2Editor : Editor
//{
//    SerializedProperty nillSpawnChance;
//    SerializedProperty spawnOptions;
//    SerializedProperty spawnerId;
//    SerializedProperty spawnCondition;
//    SerializedProperty elapsedTime;
//    //SerializedProperty spawnInterval;
//    SerializedProperty destroyAllWithSpawnerId;
//    SerializedProperty touchTriggerWithSpawnerId;

//    private void OnEnable()
//    {
//        // Link the SerializedProperties to the corresponding fields in your script
//        nillSpawnChance = serializedObject.FindProperty("nillSpawnChance");
//        spawnOptions = serializedObject.FindProperty("spawnOptions");
//        spawnerId = serializedObject.FindProperty("spawnerId");
//        spawnCondition = serializedObject.FindProperty("spawnCondition");
//        elapsedTime = serializedObject.FindProperty("elapsedTime");
//        //spawnInterval = serializedObject.FindProperty("spawnInterval");
//        destroyAllWithSpawnerId = serializedObject.FindProperty("destroyAllWithSpawnerId");
//        touchTriggerWithSpawnerId = serializedObject.FindProperty("touchTriggerWithSpawnerId");
//    }

//    public override void OnInspectorGUI()
//    {
//        // Update the serialized object
//        serializedObject.Update();

//        // Draw the common properties
//        EditorGUILayout.PropertyField(nillSpawnChance);
//        EditorGUILayout.PropertyField(spawnOptions);
//        EditorGUILayout.PropertyField(spawnerId);
//        EditorGUILayout.PropertyField(spawnCondition);

//        // Show/hide fields based on the selected spawnCondition
//        switch ((Level.LevelSpawn.SpawnCondition)spawnCondition.enumValueIndex)
//        {
//            case Level.LevelSpawn.SpawnCondition.ElapsedTime:
//                EditorGUILayout.PropertyField(elapsedTime);
//                break;
//            //case Spawner_NetworkObject_v2.SpawnCondition.Continuous:
//            //    EditorGUILayout.PropertyField(spawnInterval);
//            //    break;
//            case Level.LevelSpawn.SpawnCondition.PlayerDestroyAllWithSpawnerId:
//                EditorGUILayout.PropertyField(destroyAllWithSpawnerId);
//                break;
//            case Level.LevelSpawn.SpawnCondition.PlayerTouchTriggerWithSpawnerId:
//                EditorGUILayout.PropertyField(touchTriggerWithSpawnerId);
//                break;
//        }

//        // Apply any changes made in the inspector
//        serializedObject.ApplyModifiedProperties();
//    }
//}
