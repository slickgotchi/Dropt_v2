using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cysharp.Threading.Tasks;

public class PlayerCharacter : NetworkCharacter
{
    [Header("Essence Stat")]
    public float baseEssence = 1000;
    public float baseInfusedEssenceBonus = 250;
    [HideInInspector] public NetworkVariable<float> Essence = new NetworkVariable<float>(0);

    [Header("Equipment Buff Objects")]
    public BuffObject bodyBuffObject;
    public BuffObject faceBuffObject;
    public BuffObject eyesBuffObject;
    public BuffObject headBuffObject;
    public BuffObject rightHandBuffObject;
    public BuffObject leftHandBuffObject;
    public BuffObject petBuffObject;

    private SoundFX_Player m_soundFX_Player;

    [SerializeField] private GameObject m_hpCannisterEffect;
    [SerializeField] private GameObject m_essenceCannisterEffect;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            Essence.Value = baseEssence;
        }
        m_soundFX_Player = GetComponent<SoundFX_Player>();
    }

    public override void OnUpdate()
    {
        if (IsServer)
        {
            if (LevelManager.Instance.IsDegenapeVillage())
            {
                var playerOffchainData = GetComponent<PlayerOffchainData>();
                if (playerOffchainData != null)
                {
                    Essence.Value = baseEssence + (playerOffchainData.m_isEssenceInfused_gotchi.Value ? baseInfusedEssenceBonus : 0);
                }
            }
            else
            {
                Essence.Value -= Time.deltaTime;
            }

            if (Essence.Value <= 0 && !LevelManager.Instance.IsDegenapeVillage())
            {
                var playerController = GetComponent<PlayerController>();
                if (playerController != null)
                {
                    _ = playerController.KillPlayer(REKTCanvas.TypeOfREKT.Essence);
                }

            }
        }
    }

    // called by PlayerController when gotchi changes
    public void Init(int gotchiId)
    {
        InitGotchiStats(gotchiId);
        InitWearableBuffs(gotchiId);
    }

    public override void TakeDamage(float damage, bool isCritical, GameObject damageDealer = null)
    {
        base.TakeDamage(damage, isCritical, damageDealer);

        if (isCritical)
        {
            m_soundFX_Player?.PlayTakeDamageBigSound();
        }
        else
        {
            m_soundFX_Player?.PlayTakeDamageSmallSound();
        }
    }

    private void InitGotchiStats(int gotchiId)
    {
        if (!IsLocalPlayer) return;

        // get gotchi data
        var gotchiData = GotchiHub.GotchiDataManager.Instance.GetGotchiDataById(gotchiId);
        if (gotchiData != null)
        {
            InitGotchiStatsServerRpc(gotchiData.numericTraits);
            return;
        }

        // if got to here, try offchain data
        var offchainGotchiData = GotchiHub.GotchiDataManager.Instance.GetOffchainGotchiDataById(gotchiId);
        if (offchainGotchiData != null)
        {
            InitGotchiStatsServerRpc(offchainGotchiData.numericTraits);
            return;
        }

        // no matches so log a warning
        Debug.LogWarning("No gotchiData avaiable for gotchi ID: " + gotchiId);
    }

    [Rpc(SendTo.Server)]
    private void InitGotchiStatsServerRpc(short[] numericTraits)
    {
        // update character stats
        var hp = DroptStatCalculator.GetDroptStatForGotchiByTraitPoints(numericTraits[0], TraitType.NRG);
        var attack = DroptStatCalculator.GetDroptStatForGotchiByTraitPoints(numericTraits[1], TraitType.AGG);
        var critChance = DroptStatCalculator.GetDroptStatForGotchiByTraitPoints(numericTraits[2], TraitType.SPK);
        var ap = DroptStatCalculator.GetDroptStatForGotchiByTraitPoints(numericTraits[3], TraitType.BRN);
        var doubleStrikeChance = DroptStatCalculator.GetDroptStatForGotchiByTraitPoints(numericTraits[4], TraitType.EYS);
        var critDamage = DroptStatCalculator.GetDroptStatForGotchiByTraitPoints(numericTraits[5], TraitType.EYC);

        baseHpMax = hp;
        baseHpCurrent = hp;
        baseHpBuffer = 0;

        baseAttackPower = attack;
        baseCriticalChance = critChance;

        baseApMax = ap;
        baseApCurrent = ap;
        baseApBuffer = 0;

        baseDoubleStrikeChance = doubleStrikeChance;

        baseCriticalDamage = critDamage;

        //Debug.Log("Setting player stats, hp: " + hp + ", atk: " + attack + ", crit: " + critChance + ", ap: " + ap + ", doubleStrike: " + doubleStrikeChance + ", critDmg: " + critDamage);

        RecalculateStats();
    }

    private void InitWearableBuffs(int gotchiId)
    {
        if (!IsLocalPlayer) return;

        // get gotchi data
        var gotchiData = GotchiHub.GotchiDataManager.Instance.GetGotchiDataById(gotchiId);
        if (gotchiData != null)
        {
            InitWearableBuffsServerRpc(gotchiData.equippedWearables);
            return;
        }

        // try offchain gotchi data
        var offchainGotchiData = GotchiHub.GotchiDataManager.Instance.GetOffchainGotchiDataById(gotchiId);
        if (offchainGotchiData != null)
        {
            InitWearableBuffsServerRpc(offchainGotchiData.equippedWearables);
            return;
        }

        Debug.LogWarning("No gotchiData avaiable for gotchi ID: " + gotchiId);
    }

    [Rpc(SendTo.Server)]
    private void InitWearableBuffsServerRpc(ushort[] equipment)
    {
        // remove all existing buffs
        RemoveBuffObject(bodyBuffObject);
        RemoveBuffObject(faceBuffObject);
        RemoveBuffObject(eyesBuffObject);
        RemoveBuffObject(headBuffObject);
        RemoveBuffObject(rightHandBuffObject);
        RemoveBuffObject(leftHandBuffObject);
        RemoveBuffObject(petBuffObject);

        // get ref to equipment
        //var equipment = gotchiData.equippedWearables;

        // create new buffs based on gotchi data
        bodyBuffObject = CreateBuffObjectFromWearableId(equipment[0]);
        faceBuffObject = CreateBuffObjectFromWearableId(equipment[1]);
        eyesBuffObject = CreateBuffObjectFromWearableId(equipment[2]);
        headBuffObject = CreateBuffObjectFromWearableId(equipment[3]);
        rightHandBuffObject = CreateBuffObjectFromWearableId(equipment[4]);
        leftHandBuffObject = CreateBuffObjectFromWearableId(equipment[5]);
        petBuffObject = CreateBuffObjectFromWearableId(equipment[6]);

        // add the buffs
        AddBuffObject(bodyBuffObject);
        AddBuffObject(faceBuffObject);
        AddBuffObject(eyesBuffObject);
        AddBuffObject(headBuffObject);
        AddBuffObject(rightHandBuffObject);
        AddBuffObject(leftHandBuffObject);
        AddBuffObject(petBuffObject);
    }

    [Rpc(SendTo.Server)]
    public void SetWearableBuffServerRpc(PlayerEquipment.Slot slot, int wearableId)
    {
        switch (slot)
        {
            case PlayerEquipment.Slot.Body:
                RemoveBuffObject(bodyBuffObject);
                bodyBuffObject = CreateBuffObjectFromWearableId(wearableId);
                AddBuffObject(bodyBuffObject);
                break;
            case PlayerEquipment.Slot.Face:
                RemoveBuffObject(faceBuffObject);
                faceBuffObject = CreateBuffObjectFromWearableId(wearableId);
                AddBuffObject(faceBuffObject);
                break;
            case PlayerEquipment.Slot.Eyes:
                RemoveBuffObject(eyesBuffObject);
                eyesBuffObject = CreateBuffObjectFromWearableId(wearableId);
                AddBuffObject(eyesBuffObject);
                break;
            case PlayerEquipment.Slot.Head:
                RemoveBuffObject(headBuffObject);
                headBuffObject = CreateBuffObjectFromWearableId(wearableId);
                AddBuffObject(headBuffObject);
                break;
            case PlayerEquipment.Slot.RightHand:
                RemoveBuffObject(rightHandBuffObject);
                rightHandBuffObject = CreateBuffObjectFromWearableId(wearableId);
                AddBuffObject(rightHandBuffObject);
                break;
            case PlayerEquipment.Slot.LeftHand:
                RemoveBuffObject(leftHandBuffObject);
                leftHandBuffObject = CreateBuffObjectFromWearableId(wearableId);
                AddBuffObject(leftHandBuffObject);
                break;
            case PlayerEquipment.Slot.Pet:
                RemoveBuffObject(petBuffObject);
                petBuffObject = CreateBuffObjectFromWearableId(wearableId);
                AddBuffObject(petBuffObject);
                break;
            default: break;
        }
    }

    private BuffObject CreateBuffObjectFromWearableId(int wearableId)
    {
        BuffObject buffObject = ScriptableObject.CreateInstance<BuffObject>();
        Wearable wearable = WearableManager.Instance.GetWearable(wearableId);
        if (wearable == null)
        {
            return buffObject;
        }

        // Ensure the buffs list is initialized
        if (buffObject.buffs == null)
        {
            buffObject.buffs = new List<Buff>();
        }

        var hp = DroptStatCalculator.GetDroptStatForWearableByTraitPoints(wearable.Nrg, wearable.Rarity, TraitType.NRG);
        var atk = DroptStatCalculator.GetDroptStatForWearableByTraitPoints(wearable.Agg, wearable.Rarity, TraitType.AGG);
        var crit = DroptStatCalculator.GetDroptStatForWearableByTraitPoints(wearable.Spk, wearable.Rarity, TraitType.SPK);
        var ap = DroptStatCalculator.GetDroptStatForWearableByTraitPoints(wearable.Brn, wearable.Rarity, TraitType.BRN);

        //Debug.Log("Create buff from wearable: " + wearable.Name + ", hp + " + hp + ", atk + " + atk + ", crit + " + crit + ", ap + " + ap);

        // hp buff
        buffObject.buffs.Add(new Buff
        {
            BuffModifier = Buff.Modifier.Add,
            BuffStat = CharacterStat.HpMax,
            Value = hp
        });

        // attack buff
        buffObject.buffs.Add(new Buff
        {
            BuffModifier = Buff.Modifier.Add,
            BuffStat = CharacterStat.AttackPower,
            Value = atk
        });

        // crit chance buff
        buffObject.buffs.Add(new Buff
        {
            BuffModifier = Buff.Modifier.Add,
            BuffStat = CharacterStat.CriticalChance,
            Value = crit
        });

        // ap buff
        buffObject.buffs.Add(new Buff
        {
            BuffModifier = Buff.Modifier.Add,
            BuffStat = CharacterStat.ApMax,
            Value = ap
        });

        return buffObject;
    }

    public void RecoverHealthByPercentageOfTotalHp(float percentage)
    {
        if (!IsServer)
        {
            Debug.LogWarning("Can not recover health on client");
            return;
        }

        currentDynamicStats.HpCurrent += currentStaticStats.HpMax * percentage / 100;
        if (currentDynamicStats.HpCurrent > currentStaticStats.HpMax)
        {
            currentDynamicStats.HpCurrent = currentStaticStats.HpMax;
        }
    }

    public bool IsHpFullyCharged()
    {
        return currentDynamicStats.HpCurrent == currentStaticStats.HpMax;
    }

    public void AddEssenceValue(int amount)
    {
        Essence.Value += amount;
    }

    public void SpawnHpCannistaerEffect()
    {
        SpawnEffect(m_hpCannisterEffect);
    }

    public void SpawnEssenceCannisterEffect()
    {
        SpawnEffect(m_essenceCannisterEffect);
    }

    private async void SpawnEffect(GameObject prefab)
    {
        GameObject effect = Instantiate(prefab, transform);
        effect.transform.localPosition = new Vector3(0, 0.5f, 0);
        NetworkObject networkObject = effect.GetComponent<NetworkObject>();
        networkObject.Spawn();
        await UniTask.Delay(1500);
        networkObject.Despawn();
    }
}