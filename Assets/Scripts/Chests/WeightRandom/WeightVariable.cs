using System;

namespace Chests.WeightRandom
{
    [Serializable]
    public class WeightVariable<TData> 
    {
        public TData Data;
        public int Chance;

        public WeightVariable()
        {
        }
        
        public WeightVariable(TData data, int chance)
        {
            Data = data;
            Chance = chance;
        }
    }
}