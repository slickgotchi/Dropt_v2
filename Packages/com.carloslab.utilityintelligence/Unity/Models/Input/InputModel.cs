#region

using System;
using System.Runtime.Serialization;
using CarlosLab.Common;
using Newtonsoft.Json;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    [JsonConverter(typeof(GenericModelConverter<InputModel>))]
    public class InputModel : GenericModelItem<InputContainerModel, Input>, IContainerItemValue
    {
        public string Category
        {
            get => (string) GetValue(nameof(Category));
            internal set => SetValue(nameof(Category), value);
        }
        public bool HasNoTarget
        {
            get => (bool) GetValue(nameof(HasNoTarget));
            internal set => SetValue(nameof(HasNoTarget), value);
        }
        
        public bool EnableCachePerTarget
        {
            get => (bool) GetValue(nameof(EnableCachePerTarget));
            internal set => SetValue(nameof(EnableCachePerTarget), value);
        }

        public Type ValueType => Runtime.ValueType;
        public object ValueObject => Runtime.ValueObject;

    }
}