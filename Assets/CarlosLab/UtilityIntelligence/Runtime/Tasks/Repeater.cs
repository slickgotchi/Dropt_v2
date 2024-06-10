using System;

namespace CarlosLab.UtilityIntelligence
{
    public sealed class Repeater : Decorator
    {
        #region Fields

        private int maxRepeatCount;
        private int currentRepeatCount;

        #endregion
        
        public event Action CurrentRepeatCountChanged;
        

        #region Properties

        public int MaxRepeatCount
        {
            get => maxRepeatCount;
            set => maxRepeatCount = value;
        }

        public int CurrentRepeatCount
        {
            get => currentRepeatCount;
            set
            {
                if (currentRepeatCount == value) return;

                currentRepeatCount = value;
                
                CurrentRepeatCountChanged?.Invoke();
            }
        }
        public bool RepeatForever => maxRepeatCount <= 0;
        public bool CanRepeat => currentRepeatCount < maxRepeatCount;

        #endregion

        protected override void OnStart()
        {
            CurrentRepeatCount = 0;
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            var status = UpdateStatus.Failure;

            if (child == null)
                return status;
            
            var childStatus = child.RunDecision(deltaTime);
            // UtilityIntelligenceConsole.Instance.Log($"Agent: {Agent.Name} Task Repeater OnUpdate childStatus: {childStatus}");

            switch (childStatus)
            {
                case RunStatus.Running:
                    status = UpdateStatus.Running;
                    break;
                case RunStatus.Failure:
                    status = UpdateStatus.Failure;
                    break;
                case RunStatus.Success:

                    if (!RepeatForever)
                    {
                        CurrentRepeatCount++;
                        if (CanRepeat)
                            status = UpdateStatus.Running;
                        else
                            status = UpdateStatus.Success;
                    }
                    else
                    {
                        status = UpdateStatus.Running;
                    }
                    break;
                case RunStatus.End:
                    status = UpdateStatus.Running;
                    break;
            }
            
            return status;
        }
    }
}