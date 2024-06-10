namespace CarlosLab.UtilityIntelligence
{
    public class AbortTaskCommand : DecisionChangeContextCommand
    {
        public override void Execute(DecisionContext oldContext, DecisionContext newContext)
        {
            if (!newContext.Decision.HasNoTarget && oldContext.Target != newContext.Target)
            {
                // UtilityIntelligenceConsole.Instance.Log("DecisionTaskPreContextChangedStrategy Abort Decision Task");
                oldContext.Task?.Abort();
            }
        }
    }
}