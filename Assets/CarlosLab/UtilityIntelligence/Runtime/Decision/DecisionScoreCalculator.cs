#region

using System;
using System.Collections.Generic;
using CarlosLab.Common;
using CarlosLab.Common.Extensions;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public sealed class DecisionScoreCalculator : UtilityIntelligenceMember
    {
        public DecisionScoreCalculator(float weight = 1.0f)
        {
            Weight = weight;
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
            return TryAddConsideration(considerations.Count, name, consideration);
        }

        internal bool TryAddConsideration(int index, string name, Consideration consideration)
        {
            if (string.IsNullOrEmpty(name)) return false;

            if (considerationDict.TryAdd(name, consideration))
            {
                considerations.Insert(index, consideration);
                return true;
            }

            return false;
        }

        internal bool TryRemoveConsideration(string name)
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

        public void MoveConsideration(int sourceIndex, int destIndex)
        {
            considerations.Move(sourceIndex, destIndex);
        }

        #endregion

        #region Score
        
        public float Weight { get; internal set; }

        public event Action ScoreChanged;

        private float score;
        public float Score
        {
            get => score;
            internal set
            {
                score = value;
                ScoreChanged?.Invoke();
            }
        }

        internal float CalculateScore(ref DecisionContext decisionContext, float bonus, float minToBeat)
        {
            using var _ = Profiler.Sample("CalculateScore - Decision Score Calculator");

            float finalScore = 0.0f;
            
            float score = bonus;

            float modificationFactor = Intelligence.EnableCompensationFactor
                ? ConsiderationUtils.CalculateModificationFactor(considerations.Count)
                : 0.0f;
            
            foreach (Consideration consideration in considerations)
            {
                ConsiderationContext considerationContext = consideration.HasNoTarget
                    ? consideration.NoTargetContext
                    : new(consideration, decisionContext.Target);
                if (score <= 0.0f || score <= minToBeat)
                {
                    considerationContext.CurrentStatus = ConsiderationStatus.Discarded;
                    considerationContext.FinalScore = 0.0f;
                    finalScore = 0.0f;
                }
                else
                {
                    float considerationScore = consideration.HasNoTarget
                        ? consideration.Score
                        : consideration.CalculateScore(ref considerationContext);

                    float finalConsiderationScore = Intelligence.EnableCompensationFactor ? ConsiderationUtils.CompensateScore(in considerationScore, in modificationFactor) : considerationScore;

                    considerationContext.CurrentStatus = ConsiderationStatus.Executed;
                    considerationContext.FinalScore = finalConsiderationScore;

                    score *= finalConsiderationScore;
                    finalScore = score;
                }

                decisionContext.AddConsideration(consideration.Name, considerationContext);
            }
            
            decisionContext.Score = finalScore;
            // UtilityAIConsole.Instance.Log($"DSE Decision: {Context.Decision.Name} Score: {finalScore}");
            return finalScore;
        }

        #endregion

        #region Consideration Contexts

        private readonly Dictionary<DecisionContext, Dictionary<string, ConsiderationContext>> decisionContexts = new();

        internal Dictionary<string, ConsiderationContext> GetConsiderationContexts(DecisionContext decisionContext)
        {
            if (decisionContexts.TryGetValue(decisionContext, out var considerationContexts))
                return considerationContexts;

            if (Intelligence.IsEditorOpening)
            {
                considerationContexts = new();
                decisionContexts.Add(decisionContext, considerationContexts);
                return considerationContexts;
            }

            return null;
        }

        internal void ClearContexts()
        {
            if(Intelligence.IsEditorOpening)
                decisionContexts.Clear();
        }

        #endregion
    }
}