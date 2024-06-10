using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace CarlosLab.Common
{
    public abstract class DataAsset<TModel, TRuntime>
        : DataAsset, IDataAsset<TRuntime>
        where TModel : TRootModel, IModel<TRuntime>, new()
        where TRuntime : class
    {
        #region Fields

        [NonSerialized]
        private TModel model;
        
        [NonSerialized]
        private bool isRuntimeAsset;
        
        [NonSerialized]
        private bool isEditorOpening;

        #endregion

        public override void ClearModel()
        {
            Record("Clear Model", () =>
            {
                model = new TModel();
                UpdateModelData(model);
            });
        }

        public override void ImportModel(string serializedModel)
        {
            Record("Import Model", () => DeserializeModel(serializedModel));
        }

        #region Properties

        public override string FormattedSerializedModel => DataSerializer.Format(serializedModel);

        public override object ModelObject => model;

        public TModel Model
        {
            get
            {
                if (model == null)
                {
                    // Debug.Log("JsonAsset Create Model");
                    if (string.IsNullOrEmpty(serializedModel))
                        model = new TModel();
                }

                return model;
            }
        }

        public TRuntime Runtime => model?.Runtime;
        
        public override bool IsRuntimeAsset
        {
            get => isRuntimeAsset;
            set
            {
                isRuntimeAsset = value;
                
                if(model != null)
                    model.IsRuntimeAsset = value;
            }
        }

        public override bool IsEditorOpening
        {
            get => isEditorOpening;
            set
            {
                isEditorOpening = value;
                if(model != null)
                    model.IsEditorOpening = value;
            }
        }

        protected virtual StreamingContext Context => new(StreamingContextStates.All, this);

        #endregion

        #region Serialize Functions

        public override void SerializeModel()
        {
            SerializeModel(model);
        }

        private void SerializeModel(TModel model)
        {
            if (model == null) return;

            using var _ = Profiler.Sample("DataAsset - SerializeModel");

            OnStartSerialization();

            model.DataVersion = DataVersion;
            model.FrameworkVersion = FrameworkVersion;
            serializedModel = DataSerializer.Serialize(model, Context);

            OnFinishSerialization();
        }

        protected virtual void OnStartSerialization()
        {
        }

        protected virtual void OnFinishSerialization()
        {
        }

        #endregion

        #region Deserialize Functions

        public override void DeserializeModel()
        {
            DeserializeModel(serializedModel);
        }

        private void DeserializeModel(string serializedModel)
        {
            if (string.IsNullOrEmpty(serializedModel))
                return;
            
            using var _ = Profiler.Sample("DataAsset - DeserializeModel");

            OnStartDeserialization();

            TModel model = DataSerializer.Deserialize<TModel>(serializedModel, Context);

            if (model != null)
                this.model = model;

            OnFinishDeserialization();
            OnDeserializationFinished();
        }

        protected virtual void OnStartDeserialization()
        {
        }

        protected virtual void OnFinishDeserialization()
        {
        }
        
        private void OnDeserializationFinished()
        {
            UpdateModelData(this.model);
        }

        private void UpdateModelData(TModel newModel)
        {
            if (newModel != null)
            {
                newModel.IsEditorOpening = IsEditorOpening;
                newModel.IsRuntimeAsset = IsRuntimeAsset;
            }
        }

        #endregion
    }
}