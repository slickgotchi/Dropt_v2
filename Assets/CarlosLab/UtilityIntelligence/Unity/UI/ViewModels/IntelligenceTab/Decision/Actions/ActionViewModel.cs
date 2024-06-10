#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;
using Unity.Properties;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ActionViewModel : BaseItemViewModel<ActionModel>, ITypeNameViewModel, IStatusViewModel
    {
        public event Action<Status> StatusChanged;

        public ActionViewModel(IDataAsset asset, ActionModel model) : base(asset, model)
        {
        }

        public string TypeName => Model.Runtime.GetType().Name;
        
        public Status CurrentStatus => Model.Runtime.CurrentStatus;

        protected override void RegisterModelEvents(ActionModel model)
        {
            if (Asset.IsRuntimeAsset)
                model.Runtime.StatusChanged += OnStatusChanged;
        }

        protected override void UnregisterModelEvents(ActionModel model)
        {
            model.Runtime.StatusChanged -= OnStatusChanged;
        }

        // protected override void OnModelChanged(ActionTask newModel)
        // {
        //     UpdateViewHashCode();
        // }

        private void OnStatusChanged(Status newStatus)
        {
            StatusChanged?.Invoke(newStatus);
        }

    }
}