using System;

namespace CarlosLab.UtilityIntelligence
{
    public class BaseTask : ITask
    {
        public BaseTask()
        {
            taskInternal = this;
        }

        #region Status

        private Status currentStatus = Status.Start;
        public event Action<Status> StatusChanged;
        public Status CurrentStatus
        {
            get => currentStatus;
            private set
            {
                if (currentStatus == value)
                    return;

                currentStatus = value;
                
                OnStatusChanged();
                StatusChanged?.Invoke(currentStatus);
            }
        }
        
        public bool IsRunning => currentStatus == Status.Running;

        public bool IsEnd => currentStatus == Status.End;
        
        protected virtual void OnStatusChanged()
        {
            
        }

        #endregion

        #region Task
        
        private readonly ITask taskInternal;

        RunStatus ITask.Run(float deltaTime)
        {
            var status = currentStatus;

            var runningStatus = RunStatus.Start;
            
            switch (status)
            {
                case Status.Start:
                case Status.End:
                    OnStart();
                    runningStatus = RunStatus.Running;
                    break;
                case Status.Running:
                    runningStatus = (RunStatus) OnUpdate(deltaTime);
                    break;
                case Status.Success:
                case Status.Failure:
                case Status.Aborted:
                    OnEnd();
                    runningStatus = RunStatus.End;
                    break;
            }
            
            
            CurrentStatus = (Status) runningStatus;
            return runningStatus;
        }

        void ITask.Abort()
        {
            if (currentStatus == Status.Running)
            {
                OnAbort();
                CurrentStatus = Status.Aborted;
            }
        }

        void ITask.End()
        {
            if (currentStatus == Status.Success 
                || currentStatus == Status.Failure 
                || currentStatus == Status.Aborted)
            {
                OnEnd();
                CurrentStatus = Status.End;
            }
        }
        
        internal RunStatus RunDecision(float deltaTime)
        {
            return taskInternal.Run(deltaTime);
        }
        
        internal void Abort()
        {
            taskInternal.Abort();
        }
        
        internal void End()
        {
            taskInternal.End();
        }

        internal void Reset()
        {
            CurrentStatus = Status.Start;
        }
        
        protected virtual void OnStart()
        {
        }
        
        protected virtual UpdateStatus OnUpdate(float deltaTime)
        {
            return UpdateStatus.Running;
        }
        
        //OnAbort is called when the action's target changes or when the agent makes a new decision
        protected virtual void OnAbort()
        {
        }
        
        //OnEnd is called after the action returns a success or failure
        protected virtual void OnEnd()
        {
        }

        #endregion
    }
}