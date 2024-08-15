namespace CarlosLab.UtilityIntelligence.Examples
{
    public class OtherTeamFilter : TargetFilter
    {
        protected override bool OnFilterTarget(UtilityEntity target)
        {
            if (target.EntityFacade is Character targetCharacter)
            {
                Character myCharacter = AgentFacade as Character;
                return myCharacter.Team != targetCharacter.Team;
            }

            return false;
        }
    }
}