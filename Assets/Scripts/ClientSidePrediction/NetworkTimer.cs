using Unity.Mathematics;

namespace Dropt
{
    public class NetworkTimer
    {
        float timer;
        float elapsedTime;
        public float MinTimeBetweenTicks { get; }
        public int CurrentTick { get; private set; }
        public TickAndFraction CurrentTickAndFraction;

        public NetworkTimer(float serverTickRate)
        {
            MinTimeBetweenTicks = 1f / serverTickRate;
        }

        public void Update(float deltaTime)
        {
            timer += deltaTime;
            elapsedTime += deltaTime;

            // calc current tick and fraction (NOTE: current tick and currenttickAndfraction.currenttick not always the same!)
            CurrentTickAndFraction.Tick = (int)math.floor(elapsedTime / MinTimeBetweenTicks);
            CurrentTickAndFraction.Fraction = (elapsedTime - CurrentTickAndFraction.Tick * MinTimeBetweenTicks) / MinTimeBetweenTicks;
        }

        public bool ShouldTick()
        {
            if (timer >= MinTimeBetweenTicks)
            {
                timer -= MinTimeBetweenTicks;
                CurrentTick++;
                return true;
            }

            return false;
        }

        public struct TickAndFraction
        {
            public int Tick;
            public float Fraction;
        }
    }
}