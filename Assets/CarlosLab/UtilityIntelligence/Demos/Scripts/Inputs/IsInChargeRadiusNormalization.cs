namespace CarlosLab.UtilityIntelligence.Demos
{
    public class IsInChargeRadiusNormalization : InputNormalization<float>
    {
        public float ChargeRadius = 2;

        protected override float OnCalculateNormalizedInput(float rawInput, InputContext context)
        {
            return rawInput >= 0 && rawInput <= GetChargeRadius(context) ? 1.0f : 0.0f;
        }

        private float GetChargeRadius(InputContext context)
        {
            if (context is { TargetFacade: ChargeStation chargeStation })
                return chargeStation.ChargeRadius;
            return ChargeRadius;
        }
    }
}