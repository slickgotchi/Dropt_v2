namespace CarlosLab.UtilityIntelligence.Demos
{
    public class ChargeStationFilter : TargetFilter
    {
        public ChargeStationType Type;

        protected override bool OnFilterTarget(UtilityEntity target)
        {
            return target.EntityFacade is ChargeStation station && station.Type == Type;
        }
    }
}