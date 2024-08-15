namespace CarlosLab.Common
{
    public interface IRootObjectMember<TRootObject> : IRootObjectComponent
        where TRootObject : class, IRootObject
    {
        TRootObject RootObject { get; set; }
    }
}