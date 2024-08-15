#region

using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class ChargeStation : UtilityEntityFacade
    {
        [SerializeField]
        private ChargeStationType type;

        [SerializeField]
        private float chargeRadius;

        [SerializeField]
        private int chargePerSec;

        public ChargeStationType Type => type;
        public float ChargeRadius => chargeRadius;
        public int ChargePerSec => chargePerSec;
    }
}