#region

using System.Diagnostics;
using System.IO;
using CarlosLab.Common.Editor;
using CarlosLab.UtilityIntelligence.UI;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

#endregion

namespace CarlosLab.UtilityIntelligence.Editor
{
    public class UtilityIntelligenceEditor : BaseEditorWindow
    {
        #region Window Functions

        public static void OpenWindow(UtilityIntelligenceAsset asset)
        {
            if (asset == null)
            {
                EditorUtility.DisplayDialog("Error!", "Cannot open Intelligence Editor Window! The asset is null",
                    "OK");
                return;
            }
            
            OpenWindow();
        }
        
        [MenuItem(UtilityIntelligenceEditorConsts.MenuPath)]
        public static void OpenWindow()
        {
            GetWindow<UtilityIntelligenceEditor>(false, FrameworkEditorConsts.FrameworkName);
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            Object openAsset = EditorUtility.InstanceIDToObject(instanceId);
            if (openAsset is UtilityIntelligenceAsset asset)
            {
                OpenWindow(asset);
                return true;
            }

            return false;
        }

        #endregion

        #region UtilityIntelligenceEditor

        private UtilityIntelligenceViewController viewController;

        protected override void OnInitGUI()
        {
            minSize = new Vector2(600, 300);
            
            LoadVisualAsset(UtilityIntelligenceEditorConsts.VisualAssetPath);
        }

            // InitView();
        protected override void OnVisualAssetLoaded()
        {
            base.OnVisualAssetLoaded();
            
            InitToolBarMenu();
            
            if (EditorGUIUtility.isProSkin)
                LoadStyleSheet(UtilityIntelligenceEditorConsts.DarkTheme);
            else
                LoadStyleSheet(UtilityIntelligenceEditorConsts.LightTheme);
            
            InitView();

            UpdateView(false);
        }

        private void InitView()
        {
            viewController = new(false);
            rootVisualElement.Add(viewController.View);
        }

        private void CloseEditor()
        {
            viewController?.CloseEditor();
        }

        private void UpdateView(bool updateModel)
        {
            bool result = UpdateViewIfSelectedObjectIsIntelligenceAsset(updateModel);

            if (!result)
                result = UpdateViewIfSelectedObjectIsAgentOwner(updateModel);

            if (result)
                return;

            CloseEditor();
        }

        private bool UpdateViewIfSelectedObjectIsIntelligenceAsset(bool updateModel)
        {
            if (Selection.activeObject is UtilityIntelligenceAsset asset)
            {
                UpdateView(asset.name, asset, updateModel);
                
                return true;
            }

            return false;
        }

        private bool UpdateViewIfSelectedObjectIsAgentOwner(bool updateModel)
        {
            if (Selection.activeGameObject != null)
            {
                UtilityAgentController agentController = Selection.activeGameObject.GetComponent<UtilityAgentController>();
                if (agentController != null && agentController.Asset != null)
                {
                    UpdateView(agentController.Name, agentController.Asset, updateModel);
                    
                    return true;
                }
            }

            return false;
        }

        private void UpdateView(string name, UtilityIntelligenceAsset asset, bool updateModel)
        {
            if (viewController == null || asset == null)
                return;
            
            frameworkVersionLabel.text = $"Framework Version: {asset.FrameworkVersion}";
            dataVersionLabel.text = $"Data Version: {asset.DataVersion}";
            
            if (!asset.IsDataVersionValid())
            {
                asset.ShowDataVersionNotCompatiblePopup();
            }

            var viewModel = viewController.ViewModel;

            if (viewModel != null && viewModel.Asset == asset)
            {
                if (updateModel) UpdateModel(asset, viewModel);
            }
            else
            {
                viewModel = viewController.CreateViewModel(asset);
            }

            viewModel.Name = name;

            viewController.UpdateView(viewModel);
        }

        private static void UpdateModel(UtilityIntelligenceAsset intelligenceAsset, UtilityIntelligenceViewModel viewModel)
        {
            UtilityIntelligenceConsole.Instance.Log("UpdateModel");
            intelligenceAsset.BlockRecording = true;

            intelligenceAsset.ResetModel();
            viewModel.Model = intelligenceAsset.Model;

            intelligenceAsset.BlockRecording = false;
        }

        #endregion

        #region Event Functions

        protected override void OnDisable()
        {
            base.OnDisable();
            CloseEditor();
        }

        protected override void OnUndoRedo()
        {
            var asset = viewController.Asset;
            if(asset == null || asset.IsRuntime)
                return;

            asset.IsInUndoRedo = true;
            UpdateView(true);
            asset.IsInUndoRedo = false;
        }

        protected override void OnEnterEditMode()
        {
            UpdateView(false);
        }
        
        //This will not be called when the focus is lost
        private void OnSelectionChange()
        {
            UpdateView(false);
        }
        
        //Update View when the focus is gained
        private void OnFocus()
        {
            UpdateView(false);
        }

        #endregion

        #region Toolbar

        private ToolbarMenu fileToolbarMenu;
        private Label frameworkVersionLabel;
        private Label dataVersionLabel;
        
        private void InitToolBarMenu()
        {
            frameworkVersionLabel = rootVisualElement.Q<Label>("FrameworkVersionLabel");
            dataVersionLabel = rootVisualElement.Q<Label>("DataVersionLabel");
            
            fileToolbarMenu = rootVisualElement.Q<ToolbarMenu>("FileToolbarMenu");
            fileToolbarMenu.menu.AppendAction("Import Data", OnFileToolBarMenu_ImportData);
            fileToolbarMenu.menu.AppendAction("Export Data", OnToolBarMenu_ExportData);
            fileToolbarMenu.menu.AppendAction("Show Data", OnToolBarMenu_ShowData);
            fileToolbarMenu.menu.AppendAction("Clear Data", OnToolBarMenu_ClearData);
        }
        
        private void OnFileToolBarMenu_ImportData(DropdownMenuAction action)
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("Cannot import data while playing");
                return;
            }

            string filePath = EditorUtility.OpenFilePanel("Import Data"
                , Application.persistentDataPath
                , FrameworkRuntimeConsts.DataExtension);

            if (string.IsNullOrEmpty(filePath)) return;
            
            if (!EditorUtility.DisplayDialog("Import Data", "All current data will be lost. Are you sure?", "YES",
                    "NO"))
                return;

            string serializedModel = File.ReadAllText(filePath);
            var asset = viewController.Asset;
            asset.ImportModel(serializedModel);

            var viewModel = viewController.CreateViewModel(asset);
            viewController.UpdateView(viewModel);
        }

        private void OnToolBarMenu_ExportData(DropdownMenuAction action)
        {
            var asset = viewController.Asset;

            string filePath = EditorUtility.SaveFilePanelInProject("Export Data"
                , asset.Name
                , FrameworkRuntimeConsts.DataExtension
                , string.Empty);

            if (!string.IsNullOrEmpty(filePath))
            {
                asset.SerializeModel();
                string formattedJson = asset.FormattedSerializedModel;
                File.WriteAllText(filePath, formattedJson);
                AssetDatabase.Refresh();
            }
        }
        
        private void OnToolBarMenu_ShowData(DropdownMenuAction action)
        {
            var asset = viewController.Asset;
            asset.SerializeModel();
            string formattedJson = asset.FormattedSerializedModel;
            string filePath = $"{Application.persistentDataPath}/{asset.Name}.json";
            File.WriteAllText(filePath, formattedJson);
            Process.Start(filePath);
        }

        private void OnToolBarMenu_ClearData(DropdownMenuAction action)
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("Cannot clear data while playing");
                return;
            }

            if (!EditorUtility.DisplayDialog("Clear Data", "This will clear all current data! Are you sure?", "YES",
                    "NO"))
                return;
            
            var asset = viewController.Asset;
            asset.ClearModel();
            CloseEditor();
            UpdateView(asset.Name, asset, false);
        }

        #endregion

        // private void OnGUI()
        // {
        //     Debug.Log("OnGUI Current Size: " + position);
        // }
    }
}