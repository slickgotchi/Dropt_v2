#region

using System;
using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class EnergyBarManager : MonoBehaviour
    {
        [SerializeField]
        private EnergyBar energyBarPrefab;

        private CharacterEnergy characterEnergy;

        private EnergyBar energyBar;
        private Transform worldCanvas;

        private void Awake()
        {
            worldCanvas = GameObject.Find("WorldCanvas").transform;
            characterEnergy = GetComponentInParent<CharacterEnergy>();
        }

        private void OnEnable()
        {
            characterEnergy.EnergyChanged += OnEnergyChanged;
        }

        private void OnDisable()
        {
            characterEnergy.EnergyChanged -= OnEnergyChanged;
        }

        private void Start()
        {
            energyBar = Instantiate(energyBarPrefab, worldCanvas, false);
            energyBar.Init(transform);
        }
        
        private void OnDestroy()
        {
            if (energyBar != null)
                Destroy(energyBar.gameObject);
        }
        
        private void OnEnergyChanged(int energy)
        {
            energyBar.UpdateEnergy(energy);
        }
    }
}