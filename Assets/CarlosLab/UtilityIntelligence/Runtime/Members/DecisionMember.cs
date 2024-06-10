#region

using System;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public class DecisionMember : UtilityAgentMember
    {
        private DecisionContext context;

        public DecisionContext Context
        {
            get => context;
            internal set
            {
                context = value;
                OnContextChanged(value);
                ContextChanged?.Invoke(value);
            }
        }
        
        public event Action<DecisionContext> ContextChanged;

        protected virtual void OnContextChanged(DecisionContext newContext)
        {
        }
    }
}