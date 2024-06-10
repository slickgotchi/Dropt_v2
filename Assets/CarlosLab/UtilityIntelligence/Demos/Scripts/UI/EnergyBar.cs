#region

using CarlosLab.Common;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.Demos
{
    public class EnergyBar : MonoBehaviour
    {
        private CharacterEnergy characterEnergy;
        private Slider energySlider;
        private TargetFollower targetFollower;

        private void Awake()
        {
            energySlider = GetComponent<Slider>();
            targetFollower = GetComponent<TargetFollower>();
        }

        public void Init(CharacterEnergy characterEnergy, Transform target)
        {
            this.characterEnergy = characterEnergy;
            this.characterEnergy.EnergyChanged += OnEnergyChanged;
            targetFollower.Init(target);
        }


        private void OnEnergyChanged(int energy)
        {
            energySlider.value = energy;
        }
    }
}