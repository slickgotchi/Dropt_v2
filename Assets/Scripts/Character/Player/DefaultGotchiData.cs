using UnityEngine;

namespace PortalDefender.AavegotchiKit
{
    [CreateAssetMenu(fileName = "DefaultGotchiData", menuName = "PortalDefender/Default Gotchi Data")]
    public class DefaultGotchiData : ScriptableObject
    {
        [Header("Gotchi Basic Information")]
        public int id = 0;
        public int hauntId = 1;
        public string gotchiName = "Default";
        public string collateral = "0xE0b22E0037B130A9F56bBb537684E6fA18192341";

        [Header("Gotchi Traits")]
        public short[] numericTraits = new short[6];
        public ushort[] equippedWearables = new ushort[8];
        public int level = 1;
        public int kinship = 0;
        public int status = 0; // 0 == portal, 1 == VRF_PENDING, 2 == open portal, 3 == Aavegotchi
        public string lastInteracted = "";

        [Header("Gotchi Sprites")]
        public Sprite spriteFront;
        public Sprite spriteBack;
        public Sprite spriteLeft;
        public Sprite spriteRight;

        public int GetTraitValue(GotchiTrait trait)
        {
            return numericTraits[(int)trait];
        }
    }

    public enum GotchiTrait
    {
        Energy,
        Aggressiveness,
        Spookiness,
        BrainSize,
        EyeShape,
        EyeColor
    }
}
