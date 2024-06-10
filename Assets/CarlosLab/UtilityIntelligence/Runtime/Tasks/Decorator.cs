namespace CarlosLab.UtilityIntelligence
{
    public abstract class Decorator : Task
    {
        protected Task child;

        public Task Child
        {
            get => child;
            internal set
            {
                if (child == value) return;

                if (child != null)
                {
                    child.Intelligence = null;
                    child.Parent = null;
                }
                
                child = value;
                
                if (child != null)
                {
                    child.Intelligence = Intelligence;
                    child.Parent = this;
                }
            }
        }
        
        #region Lifecycle Functions
        
        protected sealed override void OnAwake()
        {
            child?.Awake();
        }

        protected override void OnAbort()
        {
            child?.Abort();
        }

        protected override void OnEnd()
        {
            child?.End();
        }

        #endregion

        
        #region Event Functions

        protected override void OnIntelligenceChanged(UtilityIntelligence intelligence)
        {
            if(child != null)
                child.Intelligence = intelligence;
        }

        protected sealed override void OnContextChanged(DecisionContext context)
        {
            if(child != null)
                child.Context = context;
        }

        #endregion
    }
}