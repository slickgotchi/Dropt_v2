namespace CarlosLab.UtilityIntelligence
{
    public abstract class DecisionChangeContextCommand
    {
        public abstract void Execute(DecisionContext oldContext, DecisionContext newContext);
    }
}