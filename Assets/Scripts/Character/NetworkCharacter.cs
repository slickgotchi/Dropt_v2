using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkCharacter : NetworkBehaviour
{
    [Header("Base Stats")]
    public int baseHpMax = 100;
    public int baseHpCurrent = 100;
    public int baseHpBuffer = 0;
    public int baseAttackPower = 10;
    public float baseCriticalChance = 0.1f;
    public int baseApMax = 50;
    public int baseApCurrent = 50;
    public int baseApBuffer = 0;
    public float baseDoubleStrikeChance = 0.05f;
    public float baseCriticalDamage = 1.5f;
    public float baseMoveSpeed = 6.22f;
    public float baseAccuracy = 1f;
    public float baseEvasion = 0f;
    public float baseDamageReduction = 0f;
    public float baseApLeech = 0f;

    [Header("Damage/Health Popup Offset")]
    public Vector3 popupTextOffset = new Vector3(0, 1.5f, 0f);

    private List<BuffObject> activeBuffObjects = new List<BuffObject>();

    // NetworkVariables
    [HideInInspector] public NetworkVariable<int> HpMax = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> HpCurrent = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> HpBuffer = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> AttackPower = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<float> CriticalChance = new NetworkVariable<float>();
    [HideInInspector] public NetworkVariable<int> ApMax = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> ApCurrent = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<int> ApBuffer = new NetworkVariable<int>();
    [HideInInspector] public NetworkVariable<float> DoubleStrikeChance = new NetworkVariable<float>();
    [HideInInspector] public NetworkVariable<float> CriticalDamage = new NetworkVariable<float>();
    [HideInInspector] public NetworkVariable<float> MoveSpeed = new NetworkVariable<float>();
    [HideInInspector] public NetworkVariable<float> Accuracy = new NetworkVariable<float>();
    [HideInInspector] public NetworkVariable<float> Evasion = new NetworkVariable<float>();
    [HideInInspector] public NetworkVariable<float> DamageReduction = new NetworkVariable<float>();
    [HideInInspector] public NetworkVariable<float> ApLeech = new NetworkVariable<float>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // baseize default values on the server
            InitializeStats();
        }
    }

    public float GetAttackPower(float randomVariation = 0.1f)
    {
        return UnityEngine.Random.Range(
            AttackPower.Value * (1 - randomVariation), 
            AttackPower.Value * (1 + randomVariation));
    }

    public bool IsCriticalAttack()
    {
        var rand = UnityEngine.Random.Range(0f, 0.999f);
        return rand < CriticalChance.Value;
    }

    public virtual void TakeDamage(float damage, bool isCritical, GameObject damageDealer = null)
    {
        // reduce damage
        damage *= (1 - DamageReduction.Value);

        if (IsClient)
        {
            if (gameObject.HasComponent<SpriteFlash>())
            {
                GetComponent<SpriteFlash>().DamageFlash();
            }
        }
        if (IsServer)
        {
            HpCurrent.Value -= (int)damage;
            if (HpCurrent.Value < 0) { HpCurrent.Value = 0; }
            var position = transform.position + popupTextOffset;
            DamagePopupTextClientRpc(damage, position, isCritical);

            if (HpCurrent.Value <= 0 && IsServer)
            {
                if (gameObject.HasComponent<PlayerController>())
                {
                    ShowREKTScreenClientRpc(GetComponent<NetworkObject>().NetworkObjectId);
                    HpCurrent.Value = HpMax.Value;
                }
                else
                {
                    gameObject.GetComponent<NetworkObject>().Despawn();
                }
            }

            // do ap leech
            if (damageDealer != null)
            {
                var nc_damageDealer = damageDealer.GetComponent<NetworkCharacter>();
                if (nc_damageDealer != null)
                {
                    nc_damageDealer.ApCurrent.Value += (int)(damage * nc_damageDealer.ApLeech.Value);
                }
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ShowREKTScreenClientRpc(ulong playerNetworkObjectId)
    {
        var player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId];
        var localId = GetComponent<NetworkObject>().NetworkObjectId;
        if (player.NetworkObjectId != localId) return;

        GetComponent<PlayerPrediction>().IsInputDisabled = true;
        REKTCanvas.Instance.Show(REKTCanvas.TypeOfREKT.HP);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void DamagePopupTextClientRpc(float damage, Vector3 position, bool isCritical)
    {
        ColorUtility.TryParseHtmlString("#ffeb57", out Color critColor);

        PopupTextManager.Instance.PopupText(
            damage.ToString("F0"), 
            position, 
            isCritical ? 30 : 24, 
            isCritical ? critColor : Color.white, 
            0.2f);
    }

    protected virtual void InitializeStats()
    {
        HpMax.Value = baseHpMax;
        HpCurrent.Value = baseHpCurrent;
        HpBuffer.Value = baseHpBuffer;
        AttackPower.Value = baseAttackPower;
        CriticalChance.Value = baseCriticalChance;
        ApMax.Value = baseApMax;
        ApCurrent.Value = baseApCurrent;
        ApBuffer.Value = baseApBuffer;
        DoubleStrikeChance.Value = baseDoubleStrikeChance;
        CriticalDamage.Value = baseCriticalDamage;
        MoveSpeed.Value = baseMoveSpeed;
        Accuracy.Value = baseAccuracy;
        Evasion.Value = baseEvasion;
        DamageReduction.Value = baseDamageReduction;
        ApLeech.Value = baseApLeech;
    }

    public void AddBuffObject(BuffObject buffObject)
    {
        if (!IsServer)
        {
            Debug.LogError("Cannot call AddBuffObject() from client");
            return;
        }

        if (!activeBuffObjects.Contains(buffObject))
        {
            activeBuffObjects.Add(buffObject);
            CalculateFinalStats(); // Recalculate stats after adding a new buff
        }
    }

    public void RemoveBuffObject(BuffObject buffObject)
    {
        if (!IsServer)
        {
            Debug.LogError("Cannot call RemoveBuffObject() from client");
            return;
        }

        if (activeBuffObjects.Contains(buffObject))
        {
            activeBuffObjects.Remove(buffObject);
            CalculateFinalStats(); // Recalculate stats after removing a buff
        }
    }

    public void CalculateFinalStats()
    {
        if (!IsServer)
        {
            Debug.LogError("Cannot call CalculateFinalStats() from client");
            return;
        }

        Dictionary<CharacterStat, float> baseStats = new Dictionary<CharacterStat, float>
        {
            { CharacterStat.HpMax, baseHpMax },
            { CharacterStat.HpBuffer, 0 },
            { CharacterStat.AttackPower, baseAttackPower },
            { CharacterStat.CriticalChance, baseCriticalChance },
            { CharacterStat.ApMax, baseApMax },
            { CharacterStat.ApBuffer, 0 },
            { CharacterStat.DoubleStrikeChance, baseDoubleStrikeChance },
            { CharacterStat.CriticalDamage, baseCriticalDamage },
            { CharacterStat.MoveSpeed, baseMoveSpeed },
            { CharacterStat.Accuracy, baseAccuracy },
            { CharacterStat.Evasion, baseEvasion },
            { CharacterStat.DamageReduction, baseDamageReduction },
            { CharacterStat.ApLeech, baseApLeech },
        };

        // Apply Add/Subtract buffs
        foreach (BuffObject buffObject in activeBuffObjects)
        {
            foreach (Buff buff in buffObject.buffs)
            {
                if (buff.BuffModifier == Buff.Modifier.Add || buff.BuffModifier == Buff.Modifier.Subtract)
                {
                    if (baseStats.ContainsKey(buff.BuffStat))
                    {
                        baseStats[buff.BuffStat] += buff.BuffModifier == Buff.Modifier.Add ? buff.Value : -buff.Value;
                    }
                }
            }
        }

        // Apply Multiply/Divide buffs
        foreach (BuffObject buffObject in activeBuffObjects)
        {
            foreach (Buff buff in buffObject.buffs)
            {
                if (buff.BuffModifier == Buff.Modifier.Multiply || buff.BuffModifier == Buff.Modifier.Divide)
                {
                    if (baseStats.ContainsKey(buff.BuffStat))
                    {
                        baseStats[buff.BuffStat] *= buff.BuffModifier == Buff.Modifier.Multiply ? buff.Value : 1 / buff.Value;
                    }
                }
            }
        }

        // Apply Set buffs
        foreach (BuffObject buffObject in activeBuffObjects)
        {
            foreach (Buff buff in buffObject.buffs)
            {
                if (buff.BuffModifier == Buff.Modifier.Set)
                {
                    if (baseStats.ContainsKey(buff.BuffStat))
                    {
                        baseStats[buff.BuffStat] = buff.Value;
                    }
                }
            }
        }

        // Update network variables with final calculated stats
        HpMax.Value = (int)baseStats[CharacterStat.HpMax];
        HpBuffer.Value = (int)baseStats[CharacterStat.HpBuffer];
        AttackPower.Value = (int)baseStats[CharacterStat.AttackPower];
        CriticalChance.Value = baseStats[CharacterStat.CriticalChance];
        ApMax.Value = (int)baseStats[CharacterStat.ApMax];
        ApBuffer.Value = (int)baseStats[CharacterStat.ApBuffer];
        DoubleStrikeChance.Value = baseStats[CharacterStat.DoubleStrikeChance];
        CriticalDamage.Value = baseStats[CharacterStat.CriticalDamage];
        MoveSpeed.Value = baseStats[CharacterStat.MoveSpeed];
        Accuracy.Value = baseStats[CharacterStat.Accuracy];
        Evasion.Value = baseStats[CharacterStat.Evasion];
        DamageReduction.Value = baseStats[CharacterStat.DamageReduction];
        ApLeech.Value = baseStats[CharacterStat.ApLeech];

        // Optionally, you can log the final stats for debugging
        Debug.Log("Player Stats Updated");
    }
}