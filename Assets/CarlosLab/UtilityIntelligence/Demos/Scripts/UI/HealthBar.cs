#region

using CarlosLab.Common;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.Demos
{
    public class HealthBar : MonoBehaviour
    {
        private CharacterHealth characterHealth;
        private Slider healthSlider;
        private TargetFollower targetFollower;

        private void Awake()
        {
            healthSlider = GetComponent<Slider>();
            targetFollower = GetComponent<TargetFollower>();
        }

        public void Init(CharacterHealth characterHealth, Transform target)
        {
            this.characterHealth = characterHealth;
            this.characterHealth.HealthChanged += OnHealthChanged;
            targetFollower.Init(target);
        }


        private void OnHealthChanged(int health)
        {
            healthSlider.value = health;
        }
    }
}