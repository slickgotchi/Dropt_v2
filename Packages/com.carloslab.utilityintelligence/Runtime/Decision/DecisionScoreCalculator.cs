#region

using System.Collections.Generic;
using CarlosLab.Common.Extensions;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public partial class Decision
    {
        internal void Init()
        {
            UpdateCompensationFactor();
        }

        #region Considerations

        private readonly Dictionary<string, Consideration> considerationDict = new();

        private readonly List<Consideration> considerations = new();
        public IReadOnlyList<Consideration> Considerations => considerations;

        public bool HasConsideration(string name)
        {
            return considerationDict.ContainsKey(name);
        }

        internal bool TryAddConsideration(string name, Consideration consideration)
        {
            if (TryAddConsiderationWithoutCompensation(name, consideration))
            {
                UpdateCompensationFactor();
                return true;
            }

            return false;
        }

        internal bool TryAddConsideration(int index, string name, Consideration consideration)
        {
            if (TryAddConsiderationWithoutCompensation(index, name, consideration))
            {
                UpdateCompensationFactor();
                return true;
            }

            return false;
        }
        
        private bool TryAddConsiderationWithoutCompensation(int index, string name, Consideration consideration)
        {
            if (string.IsNullOrEmpty(name)) return false;

            if (considerationDict.TryAdd(name, consideration))
            {
                considerations.Insert(index, consideration);
                return true;
            }

            return false;
        }
        
        internal bool TryAddConsiderationWithoutCompensation(string name, Consideration consideration)
        {
            if (string.IsNullOrEmpty(name)) return false;

            if (considerationDict.TryAdd(name, consideration))
            {
                considerations.Add(consideration);
                return true;
            }

            return false;
        }

        internal bool TryRemoveConsideration(string name)
        {
            if (TryRemoveConsiderationWithoutCompensation(name))
            {
                UpdateCompensationFactor();
                return true;
            }

            return false;
        }
        
        internal bool TryRemoveConsiderationWithoutCompensation(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;

            if (considerationDict.Remove(name, out Consideration consideration))
            {
                considerations.Remove(consideration);
                return true;
            }

            return false;
        }

        public bool TryGetConsideration(string name, out Consideration consideration)
        {
            consideration = null;
            if (string.IsNullOrEmpty(name))
                return false;

            if (considerationDict.TryGetValue(name, out consideration))
                return true;

            return false;
        }

        internal void MoveConsideration(int sourceIndex, int destIndex)
        {
            considerations.Move(sourceIndex, destIndex);
        }

        #endregion

        #region Results
        
        private DecisionResult noTargetResult;
        private Dictionary<int, DecisionResult> targetResults = new(20);

        internal void Reset()
        {
            // score = 0.0f;
            noTargetResult = DecisionResult.Null;
            targetResults.Clear();
        }

        private bool TryGetResult(int targetId, out DecisionResult result)
        {
            if (targetId < 0)
            {
                result = DecisionResult.Null;
                return false;
            }

            return targetResults.TryGetValue(targetId, out result);
        }
        
        private bool TryAddResult(int targetId, DecisionResult result)
        {
            if (targetId < 0) return false;

            if (targetResults.TryAdd(targetId, result))
                return true;

            return false;
        }

        #endregion

        #region Decision

        private float weight;

        public float Weight
        {
            get => weight;
            internal set => weight = value;
        }

        #endregion

        #region Calculate Score

        private bool enableCachePerTarget;
        public bool EnableCachePerTarget
        {
            get => enableCachePerTarget;
            internal set => enableCachePerTarget = value;
        }
        
        internal float CalculateScore(float minToBeat, ref DecisionContext context)
        {
// #if CARLOSLAB_ENABLE_PROFILER
//             using var _ = Profiler.Sample("Decision - CalculateScore");
// #endif
            DecisionResult result;
            if (hasNoTarget)
            {
                CalculateScoreNoTarget(minToBeat, ref context, out result);
            }
            else
            {
                if (enableCachePerTarget)
                    CalculateScoreWithCache(minToBeat, ref context, out result);
                else
                    CalculateScoreWithoutCache(minToBeat, ref context, out result);
            }

            context.SetResult(result);
            
            float finalScore = AddMomentumBonus(result.Score, in context, in this.context);
            context.FinalScore = finalScore;
            return finalScore;
        }
        
        private void CalculateScoreNoTarget(float minToBeat, ref DecisionContext context, out DecisionResult result)
        {
            if (noTargetResult == DecisionResult.Null)
                CalculateScoreWithoutCache(minToBeat, ref context, out noTargetResult);

            result = noTargetResult;
        }
        
        private void CalculateScoreWithCache(float minToBeat, ref DecisionContext context, out DecisionResult result)
        {
            var targetId = context.Target?.Id ?? -1;

            bool getResultSuccess = TryGetResult(targetId, out result);

            if (!getResultSuccess)
            {
                CalculateScoreWithoutCache(minToBeat, ref context, out result);
                
                if(!result.Discarded)
                    TryAddResult(targetId, result);
            }
            else
            {
                // UtilityIntelligenceConsole.Instance.Log($"Ignore Calculation Decision Name: {Name}");
            }
        }

        private void CalculateScoreWithoutCache(float minToBeat, ref DecisionContext context, out DecisionResult result)
        {
            result = new(this);

            float decisionScore = 0.0f;

            // UtilityIntelligenceConsole.Instance.Log($"Decision: {decision.Name} Bonus: {score}");

            if (considerations.Count > 0)
            {
                decisionScore = weight;

                foreach (Consideration consideration in considerations)
                {
                    ConsiderationContext considerationContext = new(consideration, in context);

                    if (decisionScore <= 0.0f || decisionScore <= minToBeat)
                    {
                        considerationContext.CurrentStatus = ConsiderationStatus.Discarded;

                        decisionScore = 0.0f;

                        result.Discarded = true;
                    }
                    else
                    {
                        float considerationScore = consideration.CalculateScore(ref considerationContext);
                        
                        float finalConsiderationScore = AddCompensationFactor(considerationScore);

                        considerationContext.CurrentStatus = ConsiderationStatus.Executed;
                        considerationContext.FinalScore = finalConsiderationScore;

                        decisionScore *= finalConsiderationScore;
                    }
                    
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    context.AddConsiderationContext(consideration.Name, in considerationContext);
#endif
                }
            }

            // UtilityAIConsole.Instance.Log($"DSE Decision: {Context.Decision.Name} Score: {finalScore}");

            result.Score = decisionScore;
        }

        #endregion

        #region CompensationFactor
        
        public bool EnableCompensationFactor => rootObject.EnableCompensationFactor;

        private float compensationFactor;

        private void UpdateCompensationFactor()
        {
            compensationFactor = 1.0f - 1.0f / considerations.Count;
        }
        
        private float AddCompensationFactor(float considerationScore)
        {
            if (EnableCompensationFactor)
            {
                float makeUpValue = (1.0f - considerationScore) * compensationFactor;
                return considerationScore + considerationScore * makeUpValue;
            }

            return considerationScore;
        }
        
        #endregion

        #region MomentumBonus
        
        public float MomentumBonus => rootObject.MomentumBonus;

        internal float AddMomentumBonus(float decisionScore, in DecisionContext currentContext, in DecisionContext lastContext)
        {
            var currentDecision = currentContext.Decision;
            var currentTarget = currentContext.Target;

            var lastDecision = lastContext.Decision;
            var lastTarget = lastContext.Target;
                
            if (currentDecision == lastDecision && currentTarget == lastTarget)
            {
                decisionScore *= MomentumBonus;
            }
            
            return decisionScore;
        }

        #endregion
    }
}