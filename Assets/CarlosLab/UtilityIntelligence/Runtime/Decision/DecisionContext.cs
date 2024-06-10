#region

using System;
using System.Collections.Generic;
using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public struct DecisionContext : IContext, IEquatable<DecisionContext>
    {
        public static readonly DecisionContext Null = new();
        public readonly Decision Decision;
        public readonly UtilityEntity Target;
        public float Score;

        public DecisionContext(Decision decision, UtilityEntity target) : this()
        {
            Decision = decision;
            Target = target;
        }

        public float GetBonus(in DecisionContext last)
        {
            float bonus = Decision.ScoreCalculator.Weight;
            if (Decision.Intelligence.EnableMomentumBonus)
            {
                if (Decision == last.Decision && Target == last.Target)
                    return bonus * UtilityIntelligenceConsts.MomentumBonus;
            }

            return bonus;
        }
        
        private Dictionary<string, ConsiderationContext> considerations;

        private Dictionary<string, ConsiderationContext> Considerations
        {
            get
            {
                if (Decision == null) return null;

                if(considerations == null)
                    considerations = Decision.ScoreCalculator.GetConsiderationContexts(this);

                return considerations;
            }
        }

        public Task Task => Decision?.Task;
        public IEntityFacade TargetFacade => Target?.EntityFacade;
        
        public ConsiderationContext GetConsideration(string name)
        {
            if (Considerations != null && Considerations.TryGetValue(name, out ConsiderationContext context))
                return context;

            return ConsiderationContext.Null;
        }

        public void AddConsideration(string name, ConsiderationContext consideration)
        {
            if (Considerations == null) return;

            Considerations.TryAdd(name, consideration);
        }

        public bool Equals(DecisionContext other)
        {
            return Decision == other.Decision
                   && Target == other.Target;
        }
        
        // public bool EqualsValue(DecisionContext other)
        // {
        //     return Equals(other) 
        //            && Math.Abs(Score - other.Score) < MathUtils.Epsilon;
        // }

        public override bool Equals(object obj)
        {
            return obj is DecisionContext other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Decision, Target);
        }

        public static bool operator ==(DecisionContext left, DecisionContext right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DecisionContext left, DecisionContext right)
        {
            return !left.Equals(right);
        }
    }
}