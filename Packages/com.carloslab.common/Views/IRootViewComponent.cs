namespace CarlosLab.Common
{
    public interface IRootViewComponent
    {
        bool IsRuntime { get; }
        bool IsRuntimeUI { get; }
    }
}