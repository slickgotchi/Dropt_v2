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
            TaskConsole.Instance.Log($"Agent: {Agent.Name} Repeater OnStart childName: {child.GetType().Name} Frame: {FrameInfo.Frame}");
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            var updateStatus = UpdateStatus.Failure;

            if (child == null)
                return updateStatus;
            
            TaskConsole.Instance.Log($"Agent: {Agent.Name} Repeater OnUpdate childName: {child.GetType().Name} Frame: {FrameInfo.Frame}");

            var executeStatus = child.Execute(deltaTime);

            switch (executeStatus)
            {
                case ExecuteStatus.Running:
                    updateStatus = UpdateStatus.Running;
                    break;
                case ExecuteStatus.End:
                    var endStatus = child.EndStatus;
                    switch (endStatus)
                    {
                        case EndStatus.Failure:
                            updateStatus = UpdateStatus.Failure;
                            break;
                        case EndStatus.Aborted:
                        case EndStatus.Success:
                            if (!RepeatForever)
                            {
                                CurrentRepeatCount++;
                                if (CanRepeat)
                                    updateStatus = UpdateStatus.Running;
                                else
                                    updateStatus = UpdateStatus.Success;
                            }
                            else
                            {
                                updateStatus = UpdateStatus.Running;
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return updateStatus;
        }

        protected override void OnAbort()
        {
            TaskConsole.Instance.Log($"Agent: {Agent.Name} Repeater OnAbort childName: {child.GetType().Name} Frame: {FrameInfo.Frame}");
            base.OnAbort();
        }

        protected override void OnEnd()
        {
            TaskConsole.Instance.Log($"Agent: {Agent.Name} Repeater OnEnd childName: {child.GetType().Name} Frame: {FrameInfo.Frame}");

            base.OnEnd();
        }
    }
}