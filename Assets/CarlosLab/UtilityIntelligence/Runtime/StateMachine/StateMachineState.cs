namespace CarlosLab.UtilityIntelligence
{
    public abstract class StateMachineState<TState> : BaseStateMachineState<TState>
        where TState : class, ITask, IState
    {
        private UtilityIntelligence intelligence;

        public UtilityIntelligence Intelligence
        {
            get => intelligence;
            internal set
            {
                if (intelligence == value) return;

                intelligence = value;

                OnIntelligenceChanged(intelligence);
            }
        }

        protected virtual void OnIntelligenceChanged(UtilityIntelligence intelligence)
        {
            
        }
    }
}