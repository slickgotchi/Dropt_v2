using System;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class State : IntelligenceTask, IState
    {
        public abstract string Name { get;}
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

        protected override void OnStatusChanged()
        {
            // StateMachineConsole.Instance.Log($"Agent: {Agent.Name} StateMachine StatusChanged {Name} NewState: {CurrentStatus}");
        }
    }
}