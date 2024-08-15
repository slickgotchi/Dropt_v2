#region

using System;
using System.Runtime.Serialization;

#endregion

namespace CarlosLab.Common
{
    [DataContract]
    public abstract class ModelWithId : Model, IModelWithId
    {
        [DataMember(Name = nameof(Id))]
        private string id = Guid.NewGuid().ToString();
        public string Id => id;
        string IModelWithId.Id
        {
            get => id;
            set => id = value;
        }
    }

    [DataContract]
    public abstract class ModelWithId<TRuntime> : Model<TRuntime>, IModelWithId
        where TRuntime : class, IRuntimeObject
    {
        [DataMember(Name = nameof(Id))]
        private string id = Guid.NewGuid().ToString();

        public string Id => id;
        string IModelWithId.Id
        {
            get => id;
            set => id = value;
        }
    }
}