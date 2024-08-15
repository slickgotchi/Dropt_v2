using System;

namespace CarlosLab.Common
{
    public interface IRootViewModelMember : IRootViewModelComponent
    {
        
    }
    public interface IRootViewModelMember<TRootViewModel> : IRootViewModelMember
        where TRootViewModel : class, IRootViewModel
    {
        TRootViewModel RootViewModel { get; set; }
    }
}