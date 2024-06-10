#region

using System;
using System.Collections.Generic;
using CarlosLab.Common;
using CarlosLab.Common.Extensions;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public sealed class DecisionMaker : StateMachineState<Decision>, IContainerItem
    {
        public override bool IsRuntime => Intelligence.IsRuntimeAsset;

        #region Decisions
        
        public bool CanMakeDecision
        {
            get
            {
                if (CurrentState is { KeepRunningUntilFinished: true, IsRunning: true })
                    return false;

                return true;
            }
        }

        private readonly Dictionary<string, Decision> decisionDict = new();
        
        private readonly List<Decision> decisions = new();
        public IReadOnlyList<Decision> Decisions => decisions;

        private DecisionContext lastDecisionContext;

        public bool HasDecision(string name)
        {
            return decisionDict.ContainsKey(name);
        }

        internal bool TryAddDecision(string name, Decision decision)
        {
            return TryAddDecision(decisions.Count, name, decision);
        }

        internal bool TryAddDecision(int index, string name, Decision decision)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (decisionDict.TryAdd(name, decision))
            {
                decisions.Insert(index, decision);

                decision.Intelligence = Intelligence;
                decision.OnItemAdded(name);
                return true;
            }

            return false;
        }

        internal bool TryRemoveDecision(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (decisionDict.Remove(name, out Decision decision))
            {
                decisions.Remove(decision);
                
                decision.Intelligence = null;
                decision.OnItemRemoved();
                return true;
            }

            return false;
        }

        public Decision GetDecision(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return decisionDict.GetValueOrDefault(name);
        }

        public bool TryGetDecision(string name, out Decision decision)
        {
            decision = null;
            if (string.IsNullOrEmpty(name))
                return false;

            return decisionDict.TryGetValue(name, out decision);
        }
        
        public void MoveDecision(int sourceIndex, int destIndex)
        {
            decisions.Move(sourceIndex, destIndex);
        }

        #endregion

        #region Score
        
        private float score;

        public event Action ScoreChanged;

        public float Score
        {
            get => score;
            private set
            {
                if (Math.Abs(score - value) < MathUtils.Epsilon)
                    return;

                score = value;
                ScoreChanged?.Invoke();
            }
        }

        internal float CalculateScore(HashSet<UtilityEntity> entities, float minToBeat)
        {
            using var _ = Profiler.Sample("CalculateScore - DecisionMaker");

            float finalScore = 0.0f;
            Decision bestDecision = null;

            if (decisions.Count > 0)
            {
                float bestScore = minToBeat;

                foreach (Decision decision in decisions)
                {
                    float score;

                    var targets = decision.TargetFilters.Count == 0 ? entities : decision.GetFilteredTargets();
                
                    score = CalculateScore(decision, targets, bestScore);

                    if (score > bestScore)
                    {
                        finalScore = score;
                        
                        bestScore = score;
                        bestDecision = decision;
                    }

                    if (bestDecision == null)
                    {
                        finalScore = score;
                        bestDecision = decision;
                    }
                }
            }


            // UtilityIntelligenceConsole.Instance.Log($"Agent: {Agent.Name} DecisionMaker Change Decision From: {CurrentDecision?.Name} To: {bestDecision?.Name}");

            NextState = bestDecision;
            Score = finalScore;
            return finalScore;
        }

        private float CalculateScore(Decision decision, HashSet<UtilityEntity> targets, float minToBeat)
        {
            using var _ = Profiler.Sample("CalculateScore - Decision");

            float score = 0.0f;
            decision.ScoreCalculator.ClearContexts();
            if (targets != null)
                score = CalculateScoreWithTargets(decision, targets, minToBeat);
            else
                score = CalculateScoreWithoutTargets(decision, minToBeat);

            return score;
        }

        private float CalculateScoreWithTargets(Decision decision, HashSet<UtilityEntity> targets,float minToBeat)
        {
            float finalScore = 0.0f;
            DecisionContext bestDecisionContext = DecisionContext.Null;
            
            var scoreCalculator = decision.ScoreCalculator;

            if (targets.Count > 0)
            {
                float bestScore = minToBeat;
                foreach (UtilityEntity target in targets)
                {
                    DecisionContext context = new(decision, target);

                    // var bonus = context.GetBonus();
                    // if (bonus < bestScore)
                    //     continue;

                    float bonus = context.GetBonus(in lastDecisionContext);
                
                    float score = scoreCalculator.CalculateScore(ref context, bonus, bestScore);

                    if (score > bestScore)
                    {
                        finalScore = score;
                        
                        bestScore = score;
                        bestDecisionContext = context;
                    }

                    if (bestDecisionContext == DecisionContext.Null)
                    {
                        finalScore = score;
                        bestDecisionContext = context;
                    }
                }
            }
            else
            {
                finalScore = 0.0f;
                bestDecisionContext = new(decision, null);
            }
            
            scoreCalculator.Score = finalScore;
            decision.Context = bestDecisionContext;

            return finalScore;
        }

        private float CalculateScoreWithoutTargets(Decision decision, float minToBeat)
        {
            var scoreCalculator = decision.ScoreCalculator;
            DecisionContext context = new(decision, null);
            
            float bonus = context.GetBonus(in lastDecisionContext);
            
            float score = scoreCalculator.CalculateScore(ref context, bonus, minToBeat);

            scoreCalculator.Score = score;
            decision.Context = context;
            return score;
        }
        
        #endregion

        #region Event Functions

        protected override void OnIntelligenceChanged(UtilityIntelligence intelligence)
        {
            foreach (Decision decision in decisions)
            {
                decision.Intelligence = intelligence;
            }
        }

        protected override void OnEnter()
        {
            StateMachineConsole.Instance.Log($"Agent: {Intelligence.Name} DecisionMaker OnEnter");

            lastDecisionContext = DecisionContext.Null;
        }

        protected override void OnStateChanged(Decision currentState)
        {
            if(currentState != null)
                lastDecisionContext = currentState.Context;
            else
                lastDecisionContext = DecisionContext.Null;
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
            set => name = value;
        }

        bool IContainerItem.IsInContainer
        {
            get => isInContainer;
            set => isInContainer = value;
        }

        #endregion
    }
}