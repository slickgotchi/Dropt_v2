#region

using System;
using System.Collections.Generic;
using CarlosLab.Common;
using CarlosLab.Common.Extensions;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public sealed partial class Decision : UtilityIntelligenceMemberState, INoTargetItem
    {
        public Decision(Repeater task)
        {
            this.task = task ?? new Repeater();
        }

        #region Context

        private DecisionContext context;

        public DecisionContext Context
        {
            get => context;
            private set
            {
                context = value;
                OnContextChanged(in context);
            }
        }
        
        private DecisionContext nextContext;

        public DecisionContext NextContext
        {
            get => nextContext;
            internal set
            {
                nextContext = value;

                if (!IsRuntime)
                    Context = nextContext;
            }
        }
        
        private void OnContextChanged(in DecisionContext newContext)
        {
            if (!hasNoTarget && context.Target != newContext.Target)
            {
                task.Abort();
            }
            task.Context = newContext;
        }

        private void ClearContexts()
        {
            context = DecisionContext.Null;
            nextContext = DecisionContext.Null;
        }

        #endregion

        #region Decision
        
        private Repeater task;

        public Repeater Task => task;
        
        private bool keepRunningUntilFinished;

        public bool KeepRunningUntilFinished
        {
            get => keepRunningUntilFinished;
            internal set => keepRunningUntilFinished = value;
        }

        private bool hasNoTarget;

        public bool HasNoTarget
        {
            get => hasNoTarget;
            internal set
            {
                if (hasNoTarget == value) return;
                
                hasNoTarget = value;

                var noTargetItems = Container?.NoTargetItems;
                if (noTargetItems != null)
                {
                    if (hasNoTarget)
                        noTargetItems.Add(this);
                    else
                        noTargetItems.Remove(this);
                }
            }
        }
        
        protected override void OnRootObjectChanged(UtilityIntelligence intelligence)
        {
            task.RootObject = intelligence;
        }
        
        #endregion

        #region State

        public override bool CanGoToNextState
        {
            get
            {
                if (keepRunningUntilFinished)
                    return IsEnd;
                else
                    return true;
            }
        }

        protected override void ResetState()
        {
            ClearContexts();
        }

        protected override void OnEnter()
        {
            Context = nextContext;
            StateMachineConsole.Instance.Log($"Agent: {Intelligence.AgentName} Decision: {Name} OnEnter Frame: {FrameInfo.Frame}");
        }
        
/*
        protected override void OnStart()
        {
            StateMachineConsole.Instance.Log($"Agent: {Intelligence.AgentName} Decision: {Name} OnStart Frame: {FrameInfo.Frame}");
        }
        
        protected override void OnStatusChanged(Status oldStatus, Status newStatus)
        {
            StateMachineConsole.Instance.Log($"Agent: {Intelligence.AgentName} Decision: {Name} OnStatusChanged OldStatus: {oldStatus} NewStatus: {newStatus} Frame: {FrameInfo.Frame}");
        }
*/
        protected override void OnAbort()
        {
            StateMachineConsole.Instance.Log($"Agent: {Intelligence.AgentName} Decision: {Name} OnAbort Frame: {FrameInfo.Frame}");
            task.Abort();
        }

        protected override void OnEnd()
        {
            StateMachineConsole.Instance.Log($"Agent: {Intelligence.AgentName} Decision: {Name} OnEnd Frame: {FrameInfo.Frame}");
            task.End();
        }

        protected override void OnExit()
        {
            StateMachineConsole.Instance.Log($"Agent: {Intelligence.AgentName} Decision: {Name} OnExit Frame: {FrameInfo.Frame}");
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            var updateStatus = UpdateStatus.Failure;

            if (!hasNoTarget && (Context.Target == null || Context.Target.IsActive == false))
            {
                task.Abort();
                return updateStatus;
            }
            
            // UtilityIntelligenceConsole.Instance.Log($"Agent: {Agent.Name} Decision: {Name} OnUpdate Frame: {FrameInfo.Frame}");

            var executeStatus = task.Execute(deltaTime);
                
            switch (executeStatus)
            {
                case ExecuteStatus.Running:
                    updateStatus = UpdateStatus.Running;
                    break;
                case ExecuteStatus.End:
                    var endStatus = task.EndStatus;
                    switch (endStatus)
                    {
                        case EndStatus.Success:
                            updateStatus = UpdateStatus.Success;
                            break;
                        case EndStatus.Failure:
                            updateStatus = UpdateStatus.Failure;
                            break;
                    }
                    break;
            }
            return updateStatus;
        }

        #endregion

        #region Targets
        
        private readonly Dictionary<string, TargetFilter> targetFilterDict = new();

        private readonly List<TargetFilter> targetFilters = new();
        public IReadOnlyList<TargetFilter> TargetFilters => targetFilters;

        private HashSet<UtilityEntity> filteredTargets = new(100);
        private HashSet<UtilityEntity> intersectionTargets = new(20);

        internal HashSet<UtilityEntity> GetFilteredTargets()
        {
//#if CARLOSLAB_ENABLE_PROFILER
//            using var _ = Profiler.Sample("Decision - GetFilteredTargets");
//#endif
            if (hasNoTarget || !Intelligence.IsRuntime) return null;
            
            filteredTargets.Clear();
            if (targetFilters.Count > 0)
            {
                var targetCache = targetFilters[0].TargetCache;
                for (int i = 0; i < targetCache.Count; i++)
                {
                    UtilityEntity target = targetCache[i];
                    filteredTargets.Add(target);
                }
                
                for (int i = 1; i < targetFilters.Count; i++)
                {
                    var intersection = IntersectTargets(filteredTargets, targetFilters[i].TargetCache);
                    filteredTargets.Clear();
                    foreach (var target in intersection)
                    {
                        filteredTargets.Add(target);
                    }
                }
            }
            
            return filteredTargets;
        }
        
        private HashSet<UtilityEntity> IntersectTargets(HashSet<UtilityEntity> targets1, IReadOnlyList<UtilityEntity> targets2)
        {
#if CARLOSLAB_ENABLE_PROFILER
            using var _ = Profiler.Sample("IntersectTargets - Decision");
#endif
            intersectionTargets.Clear();

            for (int index = 0; index < targets2.Count; index++)
            {
                UtilityEntity target = targets2[index];
                if (targets1.Contains(target))
                {
                    intersectionTargets.Add(target);
                }
            }

            return intersectionTargets;
        }

        public bool HasTargetFilter(string name)
        {
            return targetFilterDict.ContainsKey(name);
        }
        
        internal bool TryAddTargetFilter(string name, TargetFilter targetFilter)
        {
            return TryAddTargetFilter(targetFilters.Count, name, targetFilter);
        }
        
        internal bool TryAddTargetFilter(int index, string name, TargetFilter targetFilter)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            
            if (targetFilterDict.TryAdd(name, targetFilter))
            {
                targetFilters.Insert(index, targetFilter);
                return true;
            }

            return false;
        }

        internal bool TryRemoveTargetFilter(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            
            if (targetFilterDict.Remove(name, out TargetFilter targetFilter))
            {
                targetFilters.Remove(targetFilter);
                return true;
            }

            return false;
        }

        public bool TryGetTargetFilter(string name, out TargetFilter targetFilter)
        {
            targetFilter = null;
            if (string.IsNullOrEmpty(name))
                return false;
            
            if (targetFilterDict.TryGetValue(name, out targetFilter))
                return true;

            return false;
        }
        
        internal void MoveTargetFilter(int sourceIndex, int destIndex)
        {
            targetFilters.Move(sourceIndex, destIndex);
        }

        #endregion

        #region IContainerItem

        private string name;

        public override string Name => name;

        string IContainerItem.Name
        {
            get => name;
            set => name = value;
        }

        public bool IsInContainer => container != null;

        private DecisionContainer container;
        public DecisionContainer Container => container;
        
        void IContainerItem.HandleItemAdded(IItemContainer container, string name)
        {
            this.container = container as DecisionContainer;
            this.name = name;
        }

        void IContainerItem.HandleItemRemoved()
        {
            this.container = null;
        }

        #endregion
    }
}