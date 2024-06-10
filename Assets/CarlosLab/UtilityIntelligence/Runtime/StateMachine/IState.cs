using System;

namespace CarlosLab.UtilityIntelligence
{
    public interface IState
    {
        string Name { get; }
        bool IsActive { get; }
        event Action<bool> ActiveChanged;
        internal void Enter();
        internal void Exit();
    }
}