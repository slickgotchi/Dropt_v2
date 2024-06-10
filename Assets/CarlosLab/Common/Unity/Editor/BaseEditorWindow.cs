#region

using UnityEngine;
using UnityEngine.UIElements;

using UnityEditor;
#endregion

namespace CarlosLab.Common.Editor
{
    public class BaseEditorWindow : EditorWindow
    {
        #region Register/Unregister Converters

        private void RegisterConverters()
        {
            ConverterGroups.RegisterGlobalConverter((ref Float2 value) => (Vector2)value);
            ConverterGroups.RegisterGlobalConverter((ref Vector2 value) => (Float2)value);

            ConverterGroups.RegisterGlobalConverter((ref Float3 value) => (Vector3)value);
            ConverterGroups.RegisterGlobalConverter((ref Vector3 value) => (Float3)value);

            ConverterGroups.RegisterGlobalConverter((ref Int2 value) => (Vector2Int)value);
            ConverterGroups.RegisterGlobalConverter((ref Vector2Int value) => (Int2)value);

            ConverterGroups.RegisterGlobalConverter((ref Int3 value) => (Vector3Int)value);
            ConverterGroups.RegisterGlobalConverter((ref Vector3Int value) => (Int3)value);
        }

        #endregion

        #region Undo/Redo Functions

        protected virtual void OnUndoRedo()
        {
        }

        #endregion

        #region VisualAsset

        protected void LoadVisualAsset(string assetPath)
        {
            var visualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
            if (visualAsset == null)
                Debug.LogError($"Failed to load VisualTreeAsset at path: {assetPath}");

            visualAsset.CloneTree(rootVisualElement);
            
            OnVisualAssetLoaded();
        }
        
        protected virtual void OnVisualAssetLoaded()
        {
            
        }
        
        protected void LoadStyleSheet(string styleSheetPath)
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            if (styleSheet == null)
                Debug.LogError($"Failed to load StyleSheet at path: {styleSheetPath}");

            rootVisualElement.styleSheets.Add(styleSheet);
        }

        #endregion

        #region GUI Functions

        public void CreateGUI()
        {
            InitGUI();
        }

        private void InitGUI()
        {
            RegisterConverters();
            OnInitGUI();
        }

        protected virtual void OnInitGUI()
        {
        }

        #endregion

        #region Life Cycle Functions

        protected virtual void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        protected virtual void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        #endregion

        #region Editor/Player Mode Functions

        private void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            switch (playModeStateChange)
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnEnteredEditMode();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnEnteredPlayerMode();
                    break;
            }
        }

        protected virtual void OnEnteredEditMode()
        {
        }

        protected virtual void OnEnteredPlayerMode()
        {
        }

        #endregion
    }
}