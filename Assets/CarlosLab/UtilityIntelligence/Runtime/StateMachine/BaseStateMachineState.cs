using System;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class BaseStateMachineState<TState> : BaseStateMachine<TState>, IState
        where TState : class, ITask, IState
    {
        private bool isActive;

        public bool IsActive
        {
            get => isActive;
            set
            {
                if (isActive == value) return;
                isActive = value;
                
                ActiveChanged?.Invoke(isActive);
            }
        }

        public event Action<bool> ActiveChanged;

        void IState.Enter()
        {
            Reset();
            OnEnter();
            IsActive = true;
        }

        void IState.Exit()
        {
            OnExit();
            IsActive = false;
        }
        
        protected virtual void OnEnter()
        {
            
        }
        
        protected virtual void OnExit()
        {
            
        }
    }
}