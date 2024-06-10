#region

using System;
using System.Collections.Generic;
using CarlosLab.Common;
using CarlosLab.Common.Extensions;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public sealed class Decision : State, IContainerItem
    {
        public Decision(DecisionScoreCalculator scoreCalculator, Repeater task)
        {
            Task = task ?? new Repeater();
            ScoreCalculator = scoreCalculator ?? new DecisionScoreCalculator();
        }

        #region Context

        private DecisionContext context;

        public DecisionContext Context
        {
            get => context;
            internal set
            {
                context = value;
                OnContextChanged(context);
                ContextChanged?.Invoke(context);
            }
        }
        
        public event Action<DecisionContext> ContextChanged;

        private void OnContextChanged(DecisionContext newContext)
        {
            // ScoreCalculator.Context = newContext;
            Task.Context = newContext;
        }

        #endregion

        #region Decision
        
        public event Action NameChanged;
        
        public bool KeepRunningUntilFinished { get; internal set; }
        public bool HasNoTarget { get; internal set; }


        public DecisionScoreCalculator ScoreCalculator { get; }
        

        protected override void OnIntelligenceChanged(UtilityIntelligence intelligence)
        {
            ScoreCalculator.Intelligence = intelligence;
            Task.Intelligence = intelligence;
        }

        #endregion

        #region Task
        
        public Repeater Task { get; internal set; }
        
        protected override void OnAbort()
        {
            Task?.Abort();
        }

        protected override void OnEnd()
        {
            Task?.End();
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            var status = UpdateStatus.Failure;
            if (Task == null)
                return status;

            if (!HasNoTarget && (Context.Target == null || Context.Target.IsActive == false))
            {
                Task.Abort();
                return status;
            }

            var taskStatus = Task.RunDecision(deltaTime);
                
            switch (taskStatus)
            {
                case RunStatus.Running:
                case RunStatus.Failure:
                case RunStatus.Success:
                    status = (UpdateStatus) taskStatus;
                    break;
            }
            return status;
        }

        #endregion
        
        #region Targets
        
        private readonly Dictionary<string, TargetFilter> targetFilterDict = new();

        private readonly List<TargetFilter> targetFilters = new();
        public IReadOnlyList<TargetFilter> TargetFilters => targetFilters;

        private HashSet<UtilityEntity> intersectionTargets = new();
        private HashSet<UtilityEntity> filteredTargets = new();
        
        public HashSet<UtilityEntity> GetFilteredTargets()
        {
            if (HasNoTarget || !Intelligence.IsRuntimeAsset) return null;
            
            filteredTargets.Clear();

            if (targetFilters.Count > 0)
            {
                using var _ = Profiler.Sample("GetFilteredTargets - Decision");

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
            using var _ = Profiler.Sample("IntersectTargets - Decision");

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
        
        public void MoveTargetFilter(int sourceIndex, int destIndex)
        {
            targetFilters.Move(sourceIndex, destIndex);
        }

        #endregion

        #region IContainerItem

        private string name;

        public override string Name => name;

        private bool isInContainer;
        public bool IsInContainer => isInContainer;

        string IContainerItem.Name
        {
            get => name;
            set
            {
                if (name == value)
                    return;

                name = value;
                NameChanged?.Invoke();
            }
        }

        bool IContainerItem.IsInContainer
        {
            get => isInContainer;
            set => isInContainer = value;
        }

        #endregion
    }
}