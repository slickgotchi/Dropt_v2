namespace CarlosLab.Common
{
    public interface IRootViewMember : IRootViewComponent
    {
        
    }
    public interface IRootViewMember<TRootView> : IRootViewMember
        where TRootView: class, IRootView
    {
        TRootView RootView { get; set; }
    }
}