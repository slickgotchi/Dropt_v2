#region

using System;
using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence.Demos
{
    public class CharacterEnergy : MonoBehaviour
    {
        private int energy = 100;

        public int Energy
        {
            get => energy;
            set
            {
                var newEnergy = value;
                
                if (newEnergy < 0)
                    newEnergy = 0;
                
                if (this.energy == newEnergy)
                    return;
                
                this.energy = newEnergy;

                EnergyChanged?.Invoke(this.energy);
            }
        }

        public event Action<int> EnergyChanged;
    }
}