﻿#region

using CarlosLab.Common;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class EnergyBar : CharacterView
    {
        private CharacterEnergy characterEnergy;
        private Slider energySlider;
        private TargetFollower targetFollower;

        private void Awake()
        {
            energySlider = GetComponent<Slider>();
            targetFollower = GetComponent<TargetFollower>();
        }

        public void Init(Transform target)
        {
            targetFollower.FollowTarget(target);

            RotateTowardsCamera();
        }

        public void UpdateEnergy(int health)
        {
            energySlider.value = health;
        }
    }
}