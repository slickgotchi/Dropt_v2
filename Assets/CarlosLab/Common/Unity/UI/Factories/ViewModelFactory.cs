#region

using System;

#endregion

namespace CarlosLab.Common.UI
{
    public static class ViewModelFactory<TViewModel> where TViewModel : class, IViewModel
    {
        public static TViewModel Create(IDataAsset asset, object model)
        {
            Type type = typeof(TViewModel);
            return Activator.CreateInstance(type, asset, model) as TViewModel;
        }

        public static TViewModel Create(Type type, IDataAsset asset, object model)
        {
            return Activator.CreateInstance(type, asset, model) as TViewModel;
        }
    }
}