using System;

namespace CarlosLab.UtilityIntelligence
{
    public interface IState
    {
        string Name { get; }
        bool CanGoToNextState { get; }
        internal void Enter();
        internal void Exit();
        internal void Reset();
    }
}