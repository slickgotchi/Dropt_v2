#region

using System;
using System.Runtime.CompilerServices;
using Unity.Profiling;
using Unity.Properties;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common.UI
{
    public abstract class ViewModel : IViewModel
    {
        public string Id { get; }
        private long viewHashCode;

        protected ViewModel(IDataAsset asset, object model)
        {
            Id = Guid.NewGuid().ToString();
            Asset = asset;
            ModelObject = model;
        }

        public Type ModelType => ModelObject.GetType();
        public IDataAsset Asset { get; }

        [CreateProperty] public virtual object ModelObject { get; }

        public event Action ModelChanged;

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        public event Action ViewHashCodeChanged;

        public void UpdateViewHashCode()
        {
            viewHashCode++;
            ViewHashCodeChanged?.Invoke();
        }

        public long GetViewHashCode()
        {
            return viewHashCode;
        }

        protected void RaiseModelChanged()
        {
            ModelChanged?.Invoke();
        }

        protected void Notify([CallerMemberName] string property = "")
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }

        public abstract void ResetModel();

        public void Record(string name, Action action, bool save = false)
        {
            Asset.Record(name, action, save);
        }
    }
}