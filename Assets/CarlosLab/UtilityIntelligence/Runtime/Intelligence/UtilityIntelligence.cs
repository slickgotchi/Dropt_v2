#region

using System.Collections.Generic;
using CarlosLab.Common;
using CarlosLab.Common.Extensions;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public sealed class UtilityIntelligence : BaseStateMachine<DecisionMaker>
    {
        #region Constructors

        public UtilityIntelligence()
        {
            TargetFilterContainer = new();
            TargetFilterContainer.Intelligence = this;
            
            InputContainer = new();
            InputContainer.Intelligence = this;
            
            ConsiderationContainer = new();
            ConsiderationContainer.Intelligence = this;

            Blackboard = new();
        }
        
        public UtilityIntelligence(TargetFilterContainer targetFilterContainer, ConsiderationContainer considerations,
            InputContainer inputContainer, Blackboard blackboard)
        {
            TargetFilterContainer = targetFilterContainer ?? new();
            TargetFilterContainer.Intelligence = this;
            
            InputContainer = inputContainer ?? new();
            InputContainer.Intelligence = this;

            
            ConsiderationContainer = considerations ?? new();
            ConsiderationContainer.Intelligence = this;

            Blackboard = blackboard ?? new();
        }

        #endregion
        
        #region Properties
        public override string Name => GetType().Name;
        
        public bool IsEditorOpening { get; internal set; }
        public bool IsRuntimeAsset { get; internal set; }
        
        public bool EnableCompensationFactor { get; internal set; }
        public bool EnableMomentumBonus { get; internal set; }

        public override bool IsRuntime => IsRuntimeAsset;

        public TargetFilterContainer TargetFilterContainer { get; internal set; }
        public ConsiderationContainer ConsiderationContainer { get; }
        public InputContainer InputContainer { get; }

        public Blackboard Blackboard { get; }

        #endregion

        #region Agent

        public UtilityAgent Agent { get; internal set; }
        
        public T GetComponent<T>()
        {
            return Agent != null ? Agent.GetComponent<T>() : default;
        }

        public T GetComponentInChildren<T>()
        {
            return Agent != null ? Agent.GetComponentInChildren<T>() : default;
        }

        #endregion

        #region DecisionMakers
        
        private readonly List<DecisionMaker> decisionMakers = new();
        public IReadOnlyList<DecisionMaker> DecisionMakers => decisionMakers;

        private readonly Dictionary<string, DecisionMaker> decisionMakerDict = new();

        public bool HasDecisionMaker(string name)
        {
            return decisionMakerDict.ContainsKey(name);
        }

        internal bool TryAddDecisionMaker(string name, DecisionMaker decisionMaker)
        {
            return TryAddDecisionMaker(decisionMakers.Count, name, decisionMaker);
        }

        internal bool TryAddDecisionMaker(int index, string name, DecisionMaker decisionMaker)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (decisionMakerDict.TryAdd(name, decisionMaker))
            {
                decisionMakers.Insert(index, decisionMaker);
                
                decisionMaker.Intelligence = this;
                decisionMaker.OnItemAdded(name);
                return true;
            }

            return false;
        }

        internal bool TryRemoveDecisionMaker(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (decisionMakerDict.Remove(name, out DecisionMaker decisionMaker))
            {
                decisionMakers.Remove(decisionMaker);
                
                decisionMaker.Intelligence = null;
                decisionMaker.OnItemRemoved();
                return true;
            }

            return false;
        }

        public DecisionMaker GetDecisionMaker(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return decisionMakerDict.GetValueOrDefault(name);
        }

        public bool TryGetDecisionMaker(string name, out DecisionMaker decisionMaker)
        {
            decisionMaker = null;
            if (string.IsNullOrEmpty(name))
                return false;

            return decisionMakerDict.TryGetValue(name, out decisionMaker);
        }
        
        public void MoveDecisionMaker(int sourceIndex, int destIndex)
        {
            decisionMakers.Move(sourceIndex, destIndex);
        }

        #endregion

        #region Make Decision
        
        public bool CanMakeDecision
        {
            get
            {
                if (CurrentState != null)
                    return CurrentState.CanMakeDecision;

                return true;
            }
        }

        internal void MakeDecision(HashSet<UtilityEntity> entities)
        {
            if (!CanMakeDecision) return;
            
            using var _ = Profiler.Sample("MakeDecision - UtilityIntelligence");

            ClearFilterCaches();
            FilterTargets(entities);
            RunNoTargetConsiderations();

            DecisionMaker decisionMaker = FindBestDecisionMaker(entities);

            NextState = decisionMaker;
        }

        private DecisionMaker FindBestDecisionMaker(HashSet<UtilityEntity> entities)
        {
            DecisionMaker bestDecisionMaker = null;

            if (decisionMakers.Count > 0)
            {
                float bestScore = 0.0f;
                foreach (DecisionMaker decisionMaker in decisionMakers)
                {
                    float score = decisionMaker.CalculateScore(entities, bestScore);
                    
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestDecisionMaker = decisionMaker;
                    }
                    
                    if (bestDecisionMaker == null)
                    {
                        bestDecisionMaker = decisionMaker;
                    }
                }
            }

            return bestDecisionMaker;
        }

        #endregion

        #region Other Functions

        private void ClearFilterCaches()
        {
            var targetFilters = TargetFilterContainer.Items;
            for (int index = 0; index < targetFilters.Count; index++)
            {
                TargetFilter targetFilter = targetFilters[index];
                targetFilter.ClearCache();
            }
        }
        
        private void FilterTargets(HashSet<UtilityEntity> entities)
        {
            if (entities == null || entities.Count == 0) return;

            var targetFilters = TargetFilterContainer.Items;
            if (targetFilters.Count == 0) return;

            for (int index = 0; index < targetFilters.Count; index++)
            {
                TargetFilter targetFilter = targetFilters[index];
                foreach (var entity in entities)
                {
                    targetFilter.FilterTarget(entity);
                }
            }
        }
        
        private void RunNoTargetConsiderations()
        {
            var considerations = ConsiderationContainer.Items;
            if (considerations.Count == 0) return;

            for (int index = 0; index < considerations.Count; index++)
            {
                Consideration consideration = considerations[index];
                if (consideration.HasNoTarget)
                {
                    ConsiderationContext context = new(consideration, null);
                    consideration.CalculateScore(ref context);
                    consideration.NoTargetContext = context;
                }
            }
        }
        #endregion
    }
}