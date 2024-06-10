#region

using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence.Demos
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

        private void Start()
        {
            energyBar = Instantiate(energyBarPrefab, worldCanvas, false);
            energyBar.Init(characterEnergy, transform);
        }

        private void OnDestroy()
        {
            if (energyBar != null)
                Destroy(energyBar.gameObject);
        }
    }
}