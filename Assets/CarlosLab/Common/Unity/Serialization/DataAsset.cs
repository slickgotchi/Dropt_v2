#region

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif
#endregion

namespace CarlosLab.Common
{
    public abstract class DataAsset : ScriptableObject, IDataAsset,
        ISerializationCallbackReceiver
    {

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        protected void Notify([CallerMemberName] string property = "")
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }

        #region Fields

        [SerializeField]
        protected string serializedModel;

        [NonSerialized]
        private IRootViewModel viewModel;

        #endregion

        #region Properties

        public string Name => name;
        public abstract object ModelObject { get; }

        public IRootViewModel ViewModel
        {
            get => viewModel;
            set
            {
                if (viewModel == value) return;

                viewModel = value;

                OnViewModelChanged(viewModel);
            }
        }

        public abstract bool IsRuntimeAsset { get; set; }
        public abstract bool IsEditorOpening { get; set; }

        public bool BlockRecording { get; set; }
        public bool IsInUndoRedo { get; set; }
        public abstract int DataVersion { get; }
        public abstract string FrameworkVersion { get; }
        public string SerializedModel => serializedModel;

        public abstract string FormattedSerializedModel { get; }

        #endregion

        #region ISerializationCallbackReceiver

        public void OnBeforeSerialize()
        {
            if (Application.isPlaying) return;
            SerializeModel();
        }

        public void OnAfterDeserialize()
        {
            DeserializeModel();
        }

        #endregion

        #region Asset Functions

        public void Record(string name, Action action, bool save = false)
        {
            if (Application.isPlaying || BlockRecording)
                action?.Invoke();
            else
            {
                CommonConsole.Instance.Log($"{GetType().Name} Record: {name}");
                UndoUtils.RecordObject(this, name);

                action?.Invoke();
                SerializeAndMarkDirty();

                if (save)
                    SaveIfDirty();
            }
        }

        private void MarkDirty()
        {
#if UNITY_EDITOR
            if (IsRuntimeAsset)
                return;
            
            EditorUtility.SetDirty(this);
#endif
        }

        private void SaveIfDirty()
        {
#if UNITY_EDITOR
            if (IsRuntimeAsset)
                return;
            
            AssetDatabase.SaveAssetIfDirty(this);
#endif
        }

        public void ResetAndSave()
        {
            if (IsRuntimeAsset)
                return;
            
            ResetViewModel();
            UpdateModel();
            Save();
        }

        private void ResetViewModel()
        {
            if (ViewModel == null)
                return;
            
            ViewModel.ResetModel();
            ViewModel = null;
        }

        private void UpdateModel()
        {
            SerializeModel();
            DeserializeModel();
        }
        
        public void ResetModel()
        {
            DeserializeModel();
        }
        
        public void Save()
        {
            if (IsRuntimeAsset)
                return;
            
            MarkDirty();
            SaveIfDirty();
        }

        public abstract void ClearModel();
        public abstract void ImportModel(string serializedModel);


        protected virtual void OnViewModelChanged(IRootViewModel newViewModel)
        {
            
        }

        #endregion

        #region Serializer's Functions

        private void SerializeAndMarkDirty()
        {
            SerializeModel();
            MarkDirty();
        }

        public abstract void SerializeModel();
        public abstract void DeserializeModel();

        #endregion
    }
}