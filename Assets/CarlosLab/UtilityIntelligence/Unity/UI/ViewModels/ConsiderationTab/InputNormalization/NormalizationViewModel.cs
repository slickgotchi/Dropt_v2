#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class NormalizationViewModel : ViewModel<InputNormalizationModel>
    {
        public string Name;

        public NormalizationViewModel(IDataAsset asset, InputNormalizationModel model) : base(asset,
            model)
        {
        }

        public Type RuntimeType => Model.RuntimeType;

        public Type ValueType => Model.ValueType;
    }
}