#region

using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using CarlosLab.Common.UI;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class UtilityIntelligenceView : BaseView<UtilityIntelligenceViewModel>
    {
        private Label dataVersionLabel;

        private Tab intelligenceTab;
        private IntelligenceTabMainView intelligenceTabMainView;
        
        private Tab targetFilterTab;
        private TargetFilterTabMainView targetFilterTabMainView;
        
        private Tab considerationTab;
        private ConsiderationTabMainView considerationTabMainView;
        
        private Tab inputTab;
        private InputTabMainView inputTabMainView;
        
        private Tab blackboardTab;
        private BlackboardTabMainView blackboardTabMainView;


#if UNITY_EDITOR
        private ToolbarMenu fileToolbarMenu;
#endif

        public UtilityIntelligenceView() : base(UIBuilderResourcePaths.UtilityIntelligenceView)
        {
            style.flexGrow = 1;
        }

        protected override void OnVisualAssetLoaded()
        {
            base.OnVisualAssetLoaded();

            dataVersionLabel = this.Q<Label>("DataVersionLabel");
            
            InitTabView();
            
#if UNITY_EDITOR
            InitToolBarMenu();

            if (EditorGUIUtility.isProSkin)
                LoadStyleSheet(UIBuilderResourcePaths.UtilityIntelligenceView_Dark);
            else
                LoadStyleSheet(UIBuilderResourcePaths.UtilityIntelligenceView_Light);
#else
            LoadStyleSheet(UIBuilderResourcePaths.AgentView_Dark);
#endif
        }

        #region TabView

        private void InitTabView()
        {
            TabView tabView = this.Q<TabView>();
            intelligenceTab = tabView.Q<Tab>("IntelligenceTab");
            targetFilterTab = tabView.Q<Tab>("TargetFilterTab");
            considerationTab = tabView.Q<Tab>("ConsiderationTab");
            inputTab = tabView.Q<Tab>("InputTab");
            blackboardTab = tabView.Q<Tab>("BlackboardTab");
            
            ResetTabView();
        }

        private void ResetTabView()
        {
            intelligenceTab.Clear();
            IntelligenceTabSplitView intelligenceTabSplitView = new();
            intelligenceTabMainView = intelligenceTabSplitView.MainView;
            intelligenceTab.Add(intelligenceTabSplitView);
            
            targetFilterTab.Clear();
            TargetFilterTabSplitView targetFilterSplitView = new();
            targetFilterTabMainView = targetFilterSplitView.MainView;
            targetFilterTab.Add(targetFilterSplitView);

            considerationTab.Clear();
            ConsiderationTabSplitView considerationsSplitView = new();
            considerationTabMainView = considerationsSplitView.MainView;
            considerationTab.Add(considerationsSplitView);

            inputTab.Clear();
            InputTabSplitView inputTabSplitView = new();
            inputTabMainView = inputTabSplitView.MainView;
            inputTab.Add(inputTabSplitView);

            blackboardTab.Clear();
            BlackboardTabSplitView blackboardView = new();
            blackboardTabMainView = blackboardView.MainView;
            blackboardTab.Add(blackboardView);
        }

        protected override void OnUpdateView(UtilityIntelligenceViewModel viewModel)
        {
            dataVersionLabel.text = $"Data Version: {viewModel?.DataVersion}";
            
            intelligenceTabMainView.UpdateView(viewModel);
            targetFilterTabMainView.UpdateView(viewModel?.TargetFilters);
            considerationTabMainView.UpdateView(viewModel?.Considerations);
            inputTabMainView.UpdateView(viewModel?.Inputs);
            blackboardTabMainView.UpdateView(viewModel?.Blackboard);
        }
        
        private void UpdateTabView()
        {
            var newModel = ViewModel.Asset.ModelObject as UtilityIntelligenceModel;
            ViewModel.Model = newModel;
            // ResetTabView();
            // UpdateView(viewModel);
        }

        #endregion

        #region FileToolBarMenu
#if UNITY_EDITOR
        
        private void InitToolBarMenu()
        {
            fileToolbarMenu = this.Q<ToolbarMenu>("FileToolbarMenu");
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
            
            if (ViewModel == null) return;
            
            var asset = ViewModel.Asset;
            if (asset == null) return;

            string filePath = EditorUtility.OpenFilePanel("Import Data"
                , Application.persistentDataPath
                , FrameworkRuntimeConsts.DataExtension);

            if (string.IsNullOrEmpty(filePath)) return;
            
            if (!EditorUtility.DisplayDialog("Import Data", "All current data will be lost. Are you sure?", "YES",
                    "NO"))
                return;

            string serializedModel = File.ReadAllText(filePath);

            asset.ImportModel(serializedModel);
            UpdateTabView();
        }

        private void OnToolBarMenu_ExportData(DropdownMenuAction action)
        {
            if (ViewModel == null) return;

            var asset = ViewModel.Asset;
            if (asset == null) return;

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
            if (ViewModel == null) return;
            
            var asset = ViewModel.Asset;
            if (asset == null) return;

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
            
            if (ViewModel == null) return;
            
            var asset = ViewModel.Asset;
            if (asset == null) return;

            if (!EditorUtility.DisplayDialog("Clear Data", "This will clear all current data! Are you sure?", "YES",
                    "NO!"))
                return;

            asset.ClearModel();
            UpdateTabView();
        }
        
#endif

        #endregion


    }
}