using System;

namespace CarlosLab.UtilityIntelligence
{
    public class DecisionTask : IntelligenceTask
    {
        #region DecisionContext

        private DecisionContext context;

        public DecisionContext Context
        {
            get => context;
            internal set
            {
                DecisionContext oldContext = context;
                DecisionContext newContext = value;

                BeforeChangeContextCommand?.Execute(oldContext, newContext);
                context = newContext;
                AfterChangeContextCommand?.Execute(oldContext, newContext);

                OnContextChanged(newContext);
                ContextChanged?.Invoke(newContext);
            }
        }
        
        public DecisionChangeContextCommand BeforeChangeContextCommand { get; internal set; }
        public DecisionChangeContextCommand AfterChangeContextCommand { get; internal set; }

        
        public event Action<DecisionContext> ContextChanged;


        protected virtual void OnContextChanged(DecisionContext newContext)
        {

        }

        #endregion
    }
}