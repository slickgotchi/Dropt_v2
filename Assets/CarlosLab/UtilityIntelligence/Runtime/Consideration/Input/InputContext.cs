using System;
using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public struct InputContext : IEquatable<InputContext>
    {
        public static readonly InputContext Null = new();

        public readonly Input Input;
        public readonly UtilityEntity Target;
        public object RawInput;
        public float NormalizedInput;
        
        public IEntityFacade TargetFacade => Target?.EntityFacade;

        
        public InputContext(Input input, UtilityEntity target) : this()
        {
            Input = input;
            Target = target;
        }
        
        public bool Equals(InputContext other)
        {
            return Input == other.Input
                   && Target == other.Target;
        }
        
        public bool EqualsValue(InputContext other)
        {
            return Equals(other)
            && RawInput == other.RawInput
            && Math.Abs(NormalizedInput - other.NormalizedInput) < MathUtils.Epsilon;
        }
        
        public override bool Equals(object obj)
        {
            return obj is InputContext other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(Input, Target);
        }
        
        public static bool operator ==(InputContext left, InputContext right)
        {
            return left.EqualsValue(right);
        }

        public static bool operator !=(InputContext left, InputContext right)
        {
            return !left.Equals(right);
        }
    }
}