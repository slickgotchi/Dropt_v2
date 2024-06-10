#region

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public sealed class Sequencer : Composite
    {
        private int currentIndex;

        protected override void OnStart()
        {
            currentIndex = 0;
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            var status = UpdateStatus.Failure;

            if (ChildCount == 0)
                return status;

            if (currentIndex < ChildCount)
            {
                Task child = children[currentIndex];
                RunStatus childStatus = child.RunDecision(deltaTime);
                // UtilityIntelligenceConsole.Instance.Log($"Agent: {Agent.Name} Task Sequence OnUpdate childName: {child.GetType().Name} childStatus: {childStatus}");

                switch (childStatus)
                {
                    case RunStatus.Running:
                        status = UpdateStatus.Running;
                        break;
                    case RunStatus.Failure:
                        status = UpdateStatus.Failure;
                        break;
                    case RunStatus.Success:
                        if (currentIndex < ChildCount - 1)
                        {
                            status = UpdateStatus.Running;
                        }
                        else
                        {
                            status = UpdateStatus.Success;
                        }
                        break;
                    case RunStatus.End:
                        currentIndex++;
                        status = UpdateStatus.Running;
                        break;
                }
            }

            return status;
        }
    }
}