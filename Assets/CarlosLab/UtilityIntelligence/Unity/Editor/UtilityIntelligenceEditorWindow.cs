#region

using CarlosLab.Common.Editor;
using CarlosLab.Common.UI;
using CarlosLab.UtilityIntelligence.UI;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace CarlosLab.UtilityIntelligence.Editor
{
    public class UtilityIntelligenceEditorWindow : BaseEditorWindow
    {
        #region Window Functions

        [MenuItem(FrameworkEditorConsts.EditorMenuPath)]
        public static void OpenWindow()
        {
            GetWindow<UtilityIntelligenceEditorWindow>(false, FrameworkEditorConsts.FrameworkName);
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            Object openAsset = EditorUtility.InstanceIDToObject(instanceId);
            if (openAsset is UtilityIntelligenceAsset)
            {
                OpenWindow();
                return true;
            }

            return false;
        }

        #endregion

        #region View Functions

        protected override void OnInitGUI()
        {
            minSize = new Vector2(600, 300);

            InitView();

            UpdateView();
        }

        private void InitView()
        {
            UtilityIntelligenceView intelligenceView = UtilityIntelligenceEditorUtils.View;
            if (intelligenceView != null)
                rootVisualElement.Remove(intelligenceView);

            intelligenceView = new UtilityIntelligenceView();
            rootVisualElement.Add(intelligenceView);

            UtilityIntelligenceEditorUtils.SetView(intelligenceView);
        }

        private void UpdateView(bool updateModel = false)
        {
            bool result = UpdateViewIfSelectedObjectIsIntelligenceAsset(updateModel);

            if (!result)
                result = UpdateViewIfSelectedObjectIsAgentOwner(updateModel);

            if (result)
                return;

            UtilityIntelligenceEditorUtils.SetAsset(null);
            ResetView();
        }

        private bool UpdateViewIfSelectedObjectIsIntelligenceAsset(bool updateModel = false)
        {
            if (Selection.activeObject is UtilityIntelligenceAsset intelligenceAsset)
            {
                UpdateView(intelligenceAsset, updateModel);

                var viewModel = UtilityIntelligenceEditorUtils.ViewModel;
                if (viewModel != null) viewModel.Name = intelligenceAsset.name;
                
                return true;
            }

            return false;
        }

        private bool UpdateViewIfSelectedObjectIsAgentOwner(bool updateModel = false)
        {
            if (Selection.activeGameObject != null)
            {
                UtilityAgentController agentController = Selection.activeGameObject.GetComponent<UtilityAgentController>();
                if (agentController != null && agentController.Asset != null)
                {
                    UpdateView(agentController.Asset, updateModel);
                    if(UtilityIntelligenceEditorUtils.ViewModel != null)
                        UtilityIntelligenceEditorUtils.ViewModel.Name = agentController.name;
                    
                    return true;
                }
            }

            return false;
        }

        private void UpdateView(UtilityIntelligenceAsset intelligenceAsset, bool updateModel)
        {
            if (UtilityIntelligenceEditorUtils.View == null || intelligenceAsset == null)
                return;

            if (intelligenceAsset.ViewModel is UtilityIntelligenceViewModel viewModel)
            {
                if (updateModel) UpdateModel(intelligenceAsset, viewModel);
            }
            else
            {
                viewModel = ViewModelFactory<UtilityIntelligenceViewModel>.Create(intelligenceAsset, intelligenceAsset.Model);
            }

            UtilityIntelligenceEditorUtils.UpdateView(intelligenceAsset, viewModel);
        }

        private static void UpdateModel(UtilityIntelligenceAsset intelligenceAsset, UtilityIntelligenceViewModel viewModel)
        {
            UtilityIntelligenceConsole.Instance.Log("UpdateModel");
            intelligenceAsset.BlockRecording = true;

            intelligenceAsset.ResetModel();
            viewModel.Model = intelligenceAsset.Model;

            intelligenceAsset.BlockRecording = false;
        }

        private void ResetView()
        {
            if (UtilityIntelligenceEditorUtils.View is { ViewModel: not null }) InitView();
        }

        #endregion

        #region Event Functions

        protected override void OnDisable()
        {
            base.OnDisable();
            UtilityIntelligenceEditorUtils.Reset();
        }

        protected override void OnUndoRedo()
        {
            var asset = UtilityIntelligenceEditorUtils.Asset;
            if(asset == null || asset.IsRuntimeAsset)
                return;

            asset.IsInUndoRedo = true;
            UpdateView(asset, true);
            asset.IsInUndoRedo = false;
        }

        protected override void OnEnteredEditMode()
        {
            UpdateViewIfSelectedObjectIsAgentOwner();
        }
        
        private void OnFocus()
        {
            UpdateView();
        }

        private void OnSelectionChange()
        {
            UpdateView();
        }

        #endregion

        // private void OnGUI()
        // {
        //     Debug.Log("OnGUI Current Size: " + position);
        // }
    }
}