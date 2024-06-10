namespace CarlosLab.UtilityIntelligence
{
    public class IntelligenceTask : BaseTask
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
        
        public UtilityAgent Agent => intelligence.Agent;
        
        public T GetComponent<T>()
        {
            return Agent.GetComponent<T>();
        }

        public T GetComponentInChildren<T>()
        {
            return Agent.GetComponentInChildren<T>();
        }
    }
}