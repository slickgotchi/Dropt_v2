using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public partial class Wearable
{
    public enum SlotEnum { NA, Body, Face, Eyes, Head, Hand, Pet }
    public enum RarityEnum { NA, Common, Uncommon, Rare, Legendary, Mythical, Godlike }
    public enum WeaponTypeEnum { NA, Unarmed, Cleave, Smash, Pierce, Ballistic, Magic, Splash, Consume, Aura, Throw, Shield }

    public SlotEnum Slot;
    public int Id;
    public string Name;
    public NameEnum NameType;
    public RarityEnum Rarity;
    public int Nrg;
    public int Agg;
    public int Spk;
    public int Brn;
    public WeaponTypeEnum WeaponType;
    public int SpecialAp;
    public int SpecialCooldown;
    public CharacterStat SecondaryBuff;
    public float SecondaryBuffValue;
    public CharacterStat TertiaryBuff;
    public float TertiaryBuffValue;
    public string SwapDescription;
    public PlayerGotchi.Facing AttackView;
    public float AttackAngle;

    public static float GetRarityMultiplier(RarityEnum rarity)
    {
        float multiplier = 1f;
        switch (rarity)
        {
            case RarityEnum.NA: multiplier = 1.00f; break;
            case RarityEnum.Common: multiplier = 1.15f; break;
            case RarityEnum.Uncommon: multiplier = 1.36f; break;
            case RarityEnum.Rare: multiplier = 1.64f; break;
            case RarityEnum.Legendary: multiplier = 1.99f; break;
            case RarityEnum.Mythical: multiplier = 2.43f; break;
            case RarityEnum.Godlike: multiplier = 3.00f; break;
            default: break;
        }
        return multiplier;
    }

    public float RarityMultiplier
    {
        get { return GetRarityMultiplier(Rarity); }
        set { }
    }

    public static UnityEngine.Color GetColorByRarity(RarityEnum rarity)
    {
        if (rarity == RarityEnum.Common) return Dropt.Utils.Color.HexToColor("#8661fd");
        else if (rarity == RarityEnum.Uncommon) return Dropt.Utils.Color.HexToColor("#44b9cb");
        else if (rarity == RarityEnum.Rare) return Dropt.Utils.Color.HexToColor("#66bafd");
        else if (rarity == RarityEnum.Legendary) return Dropt.Utils.Color.HexToColor("#fbc56f");
        else if (rarity == RarityEnum.Mythical) return Dropt.Utils.Color.HexToColor("#fe96fe");
        else if (rarity == RarityEnum.Godlike) return Dropt.Utils.Color.HexToColor("#5bffaa");
        else return Dropt.Utils.Color.HexToColor("#ffffff");
    }

    public UnityEngine.Color RarityColor {
        get { return GetColorByRarity(Rarity);  }
        set { }
    }
}


public partial class Wearable {
    public enum NameEnum {
        _10GallonHat,
        _1337Laptop,
        _23Jersey,
        _32ETHCoin,
        _3DGlasses,
        AagentFedoraHat,
        AagentHeadset,
        AagentPistol,
        AagentShades,
        AagentShirt,
        AantennaBot,
        AastronautHelmet,
        AastronautSuit,
        AaveBoat,
        AaveFlag,
        AaveHeroMask,
        AaveHeroShirt,
        AavePlushToy,
        AlchemicaApron,
        AllSeeingEyes,
        AlluringEyes,
        AnimalSkins,
        ApeMask,
        AppleJuice,
        APYShades,
        Aviators,
        BabyBottle,
        Bandage,
        Basketball,
        BeardofDivinity,
        BeardofWisdom,
        BedtimeMilk,
        BeerHelmet,
        BigGHSTToken,
        BikerHelmet,
        BikerJacket,
        BitcoinBeanie,
        BitcoinGuitar,
        BlackJeans,
        BlockScanners,
        BlueCacti,
        BlueHawaiianShirt,
        BluePlaid,
        BowandArrow,
        BrunettePonytail,
        BushyEyebrows,
        CamoHat,
        CamoPants,
        CandyJaar,
        CaptainAaveMask,
        CaptainAaveShield,
        CaptainAaveSuit,
        CitaadelHelm,
        Clutch,
        Coconut,
        CoderdanShades,
        CoinGeckoTee,
        ComfyPoncho,
        CommonRofl,
        CommonWizardHat,
        CommonWizardStaff,
        CoolShades,
        CyborgEye,
        DAOEgg,
        DayDress,
        DoublesidedAxe,
        DragonHorns,
        DragonWings,
        EagleMask,
        ElfEars,
        EnergyGun,
        ETHLogoGlasses,
        ETHTShirt,
        EyesofDevotion,
        Faangs,
        FairyWings,
        FAKEBeret,
        FAKEShirt,
        FarmerJeans,
        FeatheredCap,
        Fireball,
        FlamingApron,
        FlowerStuds,
        FluffyPillow,
        ForgeGoggles,
        ForkedBeard,
        FoxyMask,
        FoxyTail,
        GalaxyBrain,
        GameController,
        GamerJacket,
        GeckoEyes,
        GeckoHat,
        GeishaHeadpiece,
        GemstoneRing,
        GentlemanCoat,
        GentlemanHat,
        Geo,
        GeodeSmasher,
        GldnXrossRobe,
        GMSeeds,
        GOATee,
        GodlikeCacti,
        GodlikeWizardHat,
        GodliLocks,
        GoldNecklace,
        GotchiMug,
        GuyFawkesMask,
        H4xx0rShirt,
        HaanzoKatana,
        HalfRektShirt,
        Handsaw,
        HazmatHood,
        HazmatSuit,
        Headphones,
        HeavenlyRobes,
        HookHand,
        HornedHelmet,
        HorseshoeMustache,
        ImperialMoustache,
        JaayGlasses,
        JaayHairpiece,
        JaaySuit,
        JamaicanFlag,
        JordanHair,
        JordanSuit,
        KabutoHelmet,
        Kimono,
        L2Sign,
        Lasso,
        LeatherTunic,
        LegendaryRofl,
        LegendaryWizardHat,
        LegendaryWizardStaff,
        LensnFrensPlant,
        LickBrain,
        LickEyes,
        LickTentacle,
        LickTongue,
        LilBubbleSpaceSuit,
        LilPumpDrank,
        LilPumpDreads,
        LilPumpGoatee,
        LilPumpShades,
        LilPumpThreads,
        LinkBubbly,
        LINKCube,
        LinkMessDress,
        LinkWhiteHat,
        LlamacornShirt,
        Longbow,
        M67Grenade,
        MarcHair,
        MarcOutfit,
        MarineCap,
        MarineJacket,
        Martini,
        MatrixEyes,
        MechanicalClaw,
        Milkshake,
        MinerHelmet,
        MinerJeans,
        MK2Grenade,
        Mohawk,
        Monocle,
        MudgenDiamond,
        MuttonChops,
        MythicalCacti,
        MythicalRofl,
        MythicalWizardHat,
        NailGun,
        Nimbus,
        NogaraArmor,
        Null,
        OKexSign,
        OliversSpoon,
        Overalls,
        PaintBrush,
        PaintPalette,
        PajamaHat,
        PajamaShirt,
        PaperFan,
        Parasol,
        PartyDress,
        Pickaxe,
        PillboxHat,
        PirateCoat,
        PirateHat,
        PiratePatch,
        Pitchfork,
        PixelcraftSquare,
        PixelcraftTee,
        PlasticEarrings,
        PlateArmor,
        PlateShield,
        PointyHorns,
        PolygonCap,
        PolygonShirt,
        PonchoHoodie,
        PortalMageArmor,
        PortalMageAxe,
        PortalMageBlackAxe,
        PortalMageHelmet,
        PrincessHair,
        PrincessTiara,
        PunkShirt,
        RadarEyes,
        RainbowVomit,
        RareRofl,
        RastaHat,
        RastaShirt,
        RedHair,
        RedHawaiianShirt,
        RedHeadband,
        RedPlaid,
        RedSantaHat,
        REKTSign,
        Roflnoggin,
        RoyalCrown,
        RoyalRobes,
        RoyalRofl,
        RoyalScepter,
        SafetyGlasses,
        SandboxHoodie,
        SebastienHair,
        SergeyBeard,
        SergeyEyes,
        ShaamanHoodie,
        ShaamanPoncho,
        SignalHeadset,
        Skateboard,
        SlicksSurfboard,
        SnapshotCap,
        SnapshotShirt,
        SnowCamoHat,
        SnowCamoPants,
        SpaceHelmet,
        SpiritSword,
        StaffofCharming,
        StaffofCreation,
        StaniHair,
        StaniLifejacket,
        SteampunkGoggles,
        SteampunkTrousers,
        StrawHat,
        SusButterfly,
        SushiBandana,
        SushiCoat,
        SushiKnife,
        SushiPiece,
        Sweatband,
        TaoistRobe,
        ThaaveHammer,
        ThaaveHelmet,
        ThaaveSuit,
        TintedShades,
        TinyCrown,
        TrackShorts,
        TrackSuit,
        TrezorWallet,
        uGOTCHIToken,
        UncommonCacti,
        UncommonRofl,
        UpArrow,
        UpOnlyShirt,
        UraniumRod,
        VNeckShirt,
        VoteSign,
        VoxelEyes,
        VRHeadset,
        WagieCap,
        WaifuPillow,
        WalkieTalkie,
        Waterbottle,
        WaterJug,
        WavyHair,
        WGMIShirt,
        Wine,
        WitchyCloak,
        WitchyHat,
        WitchyWand,
        WizardVisor,
        WraanglerJeans,
        XibotMohawk,
        YellowManbun,
        YoroiArmor,

        Unarmed,
        None,
    }

}