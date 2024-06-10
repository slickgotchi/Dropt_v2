#region

using System;
using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public struct ConsiderationContext : IEquatable<ConsiderationContext>
    {
        public static readonly ConsiderationContext Null = new();
        public readonly Consideration Consideration;
        public readonly UtilityEntity Target;
        public float Score;
        public float FinalScore;
        public InputContext Input;
        public ConsiderationStatus CurrentStatus;
        
        public IEntityFacade TargetFacade => Target?.EntityFacade;

        public ConsiderationContext(Consideration consideration, UtilityEntity target) : this()
        {
            Consideration = consideration;
            Target = target;
        }

        public bool Equals(ConsiderationContext other)
        {
            return Consideration == other.Consideration
                   && Target == other.Target
                   && Input == other.Input;
        }
        
        // public bool EqualsValue(ConsiderationContext other)
        // {
        //     return Consideration == other.Consideration
        //            && Target == other.Target
        //            && Input.EqualsValue(other.Input)
        //            && CurrentStatus == other.CurrentStatus
        //            && Math.Abs(Score - other.Score) < MathUtils.Epsilon;
        // }
        
        public override bool Equals(object obj)
        {
            return obj is ConsiderationContext other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(Consideration, Input, Target);
        }
        
        public static bool operator ==(ConsiderationContext left, ConsiderationContext right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ConsiderationContext left, ConsiderationContext right)
        {
            return !left.Equals(right);
        }
    }
}