#region

using System.Collections.Generic;
using CarlosLab.Common.Extensions;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public sealed class ParallelComplete : Composite
    {
        private readonly List<RunStatus> childrenStatus = new();

        protected override void OnStart()
        {
            childrenStatus.Clear();
            for (int index = 0; index < ChildCount; index++)
            {
                childrenStatus.Add(RunStatus.Start);
            }
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            if (ChildCount == 0)
                return UpdateStatus.Failure;
            
            var status = UpdateStatus.Success;
            
            bool exitLoop = false;
            for (int i = 0; i < ChildCount && !exitLoop; i++)
            {
                Task child = children[i];
                var childStatus = childrenStatus[i];
                if (childStatus != RunStatus.End)
                {
                    childStatus = child.RunDecision(deltaTime);
                    childrenStatus[i] = childStatus;
                    switch (childStatus)
                    {
                        case RunStatus.Running:
                            status = UpdateStatus.Running;
                            break;
                        case RunStatus.Success:
                            AbortChildren();
                            exitLoop = true;
                            status = UpdateStatus.Success;
                            break;
                        case RunStatus.Failure:
                            AbortChildren();
                            exitLoop = true;
                            status = UpdateStatus.Failure;
                            break;
                    }
                }
            }

            return status;
        }

        protected override void OnChildAdded(Task child)
        {
            childrenStatus.Add(RunStatus.Running);
        }

        protected override void OnChildMoved(int sourceIndex, int destIndex)
        {
            childrenStatus.Move(sourceIndex, destIndex);
        }

        protected override void OnChildRemoved(int index)
        {
            childrenStatus.RemoveAt(index);
        }
    }
}