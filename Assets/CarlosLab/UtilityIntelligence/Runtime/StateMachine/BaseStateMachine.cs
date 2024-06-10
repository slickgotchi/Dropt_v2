using System;
using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class BaseStateMachine<TState> : BaseTask 
        where TState : class, ITask, IState
    {
        private TState currentState;
        private TState nextState;
        
        public event Action<TState> BeforeChangeState;
        public event Action<TState> AfterChangeState;
        
        public abstract string Name { get;}
        
        public abstract bool IsRuntime { get; }
        
        public TState CurrentState
        {
            get => currentState;
            set
            {
                if (currentState == value) return;
                
                // StateMachineConsole.Instance.Log($"Agent: {Agent.Name} StateMachine StateChanged {Name} From State: {currentState?.Name} to State: {value?.Name}");
                
                BeforeChangeState?.Invoke(currentState);
            
                if (currentState != null) currentState.Exit();
                currentState = value;
                if (currentState != null) currentState.Enter();

                OnStateChanged(currentState);
                
                AfterChangeState?.Invoke(currentState);
            }
        }

        private bool updateState;
        public TState NextState
        {
            get => nextState;
            set
            {
                updateState = true;
                
                if (nextState == value) return;

                nextState = value;

                if (currentState != nextState)
                {
                    if (IsRuntime)
                    {
                        if (currentState == null)
                            CurrentState = nextState;
                        else
                            currentState.Abort();
                    }
                    else
                    {
                        CurrentState = nextState;
                    }
                }
            }
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            var status = UpdateStatus.Failure;

            if (currentState == null)
                return status;
            
            if (currentState.IsEnd)
            {
                if (updateState)
                {
                    if (currentState == nextState)
                        CurrentState = null;
                    
                    CurrentState = nextState;
                    
                    updateState = false;
                }
                
                status = UpdateStatus.Running;
                return status;
            }
            
            var stateStatus = currentState.Run(deltaTime);
            switch (stateStatus)
            {
                case RunStatus.Running:
                case RunStatus.Failure:
                case RunStatus.Success:
                case RunStatus.End:
                    status = UpdateStatus.Running;
                    break;
            }
        
            return status;
        }

        protected virtual void OnStateChanged(TState currentState)
        {
            
        }
        
        protected override void OnAbort()
        {
            currentState?.Abort();
        }

        protected override void OnEnd()
        {
            currentState?.End();
        }
        
        protected override void OnStatusChanged()
        {
            // StateMachineConsole.Instance.Log($"Agent: {Agent.Name} StateMachine StatusChanged {Name} NewState: {CurrentStatus}");
        }
    }
}