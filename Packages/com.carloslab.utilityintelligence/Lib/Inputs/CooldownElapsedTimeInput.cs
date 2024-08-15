using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public class CooldownElapsedTimeInput : Input<float>
    {
        public VariableReference<float> CooldownStartTime;

        protected override float OnGetRawInput(in InputContext context)
        {
            float elapsedTime = FrameInfo.Time - CooldownStartTime;
            return elapsedTime;
        }
    }
}