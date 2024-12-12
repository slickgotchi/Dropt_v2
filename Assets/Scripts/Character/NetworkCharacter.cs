using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkCharacter : NetworkBehaviour
{
    [Header("Base Stats")]
    public float baseHpMax = 100;
    public float baseHpCurrent = 100;
    public float baseHpBuffer = 0;
    public float baseAttackPower = 10;
    public float baseCriticalChance = 0.1f;
    public float baseApMax = 50;
    public float baseApCurrent = 50;
    public float baseApBuffer = 0;
    public float baseDoubleStrikeChance = 0.05f;
    public float baseCriticalDamage = 1.5f;
    public float baseMoveSpeed = 6.22f;
    public float baseAccuracy = 1f;
    public float baseEvasion = 0f;
    public float baseDamageReduction = 0f;
    public float baseApLeech = 0f;
    public float baseApRegen = 1f;
    public float baseKnockbackMutliplier = 1f;
    public float baseStunMultiplier = 1f;

    [Header("Damage/Health Popup Offset")]
    public Vector3 k_popupTextOffset = new Vector3(0, 1.5f, 0f);
    public Color ReceiveDamageColor = new Color(1, 1, 1);
    public int ReceiveDamageFontSize = 16;
    public Color ReceiveCriticalDamageColor = new Color(1, 1, 1);
    public int ReceiveCriticalDamageFontSize = 24;

    private List<BuffObject> activeBuffObjects = new List<BuffObject>();

    // list of buff names for client to use to do UI things client side
    private List<string> activeBuffNames_CLIENT = new List<string>();

    //private Action<ulong> m_onDamageToEnemy;

    // NetworkVariables
    [HideInInspector] public NetworkVariable<float> HpMax = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> HpCurrent = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> HpBuffer = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> AttackPower = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> CriticalChance = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> ApMax = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> ApCurrent = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> ApBuffer = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> DoubleStrikeChance = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> CriticalDamage = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> MoveSpeed = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> Accuracy = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> Evasion = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> DamageReduction = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> ApLeech = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> ApRegen = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> KnockbackMultiplier = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> StunMultiplier = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> EnemyShield = new NetworkVariable<float>(0);
    [HideInInspector] public NetworkVariable<float> MaxEnemyShield = new NetworkVariable<float>(0);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            // baseize default values on the server
            InitializeStats();
        }
    }

    protected virtual void Update()
    {
        if (!IsServer) return;

        // handle ap regen
        ApCurrent.Value += ApRegen.Value * Time.deltaTime;
        if (ApCurrent.Value > ApMax.Value)
        {
            ApCurrent.Value = ApMax.Value;
        }
    }

    public float GetAttackPower(float randomVariation = 0.1f)
    {
        return Random.Range(
            AttackPower.Value * (1 - randomVariation),
            AttackPower.Value * (1 + randomVariation));
    }

    public bool IsCriticalAttack()
    {
        var rand = Random.Range(0f, 0.999f);
        return rand < CriticalChance.Value;
    }

    public virtual void TakeDamage(float damage, bool isCritical, GameObject damageDealer = null)
    {
        // reduce damage
        damage *= (1 - DamageReduction.Value);

        HandleEnemyTakeDamage(damage, isCritical, damageDealer);
        HandlePlayerTakeDamage(damage, isCritical, damageDealer != null ? damageDealer.GetComponent<NetworkObject>().NetworkObjectId : 0);
    }

    // ENEMY
    public void HandleEnemyTakeDamage(float damage, bool isCritical, GameObject damageDealer = null)
    {
        ulong damageDealerNOID = damageDealer != null ? damageDealer.GetComponent<NetworkObject>().NetworkObjectId : 0;

        var enemyController = GetComponent<EnemyController>();
        if (enemyController == null) return;

        // LOCAL PLAYER
        if (IsClient)
        {
            // get the local player
            ulong localPlayerNOID = 0;
            //PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var player in Game.Instance.players)
            {
                var playerNetworkObject = player.GetComponent<NetworkObject>();
                if (playerNetworkObject != null && playerNetworkObject.IsLocalPlayer)
                {
                    localPlayerNOID = playerNetworkObject.NetworkObjectId;
                    break;
                }
            }

            if (localPlayerNOID == damageDealerNOID)
            {
                // do sprite flash
                var spriteFlash = GetComponentInChildren<SpriteFlash>();
                if (spriteFlash != null)
                {
                    spriteFlash.DamageFlash();
                }
            }
        }

        // SERVER
        if (IsServer)
        {
            EventManager.Instance.TriggerEventWithParam("OnEnemyGetDamage", damageDealerNOID);
            if (EnemyShield.Value > 0)
            {
                EnemyShield.Value--;
                damage = 0;
            }
            if (!IsHost)
            {
                HandleEnemyTakeDamageClientRpc(damage, isCritical, damageDealerNOID);
            }

            // deplete hp
            HpCurrent.Value -= (int)damage;
            if (HpCurrent.Value < 0) { HpCurrent.Value = 0; }

            // show damage text
            DamagePopupTextClientRpc(damage, isCritical);

            // check for death
            if (HpCurrent.Value <= 0 && enemyController != null)
            {
                // update the players kill count
                if (damageDealer != null)
                {
                    PlayerController playerController = damageDealer.GetComponent<PlayerController>();
                    if (playerController != null) playerController.KillEnemy();
                }

                // despawn this character
                Dropt.EnemyAI enemyAI = GetComponent<Dropt.EnemyAI>();
                if (enemyAI != null)
                {
                    // the AI class will handle despawning (and some children may not imeediately despawn)
                    enemyAI.Death(enemyAI.GetKnockbackPosition());
                }
                else
                {
                    var networkObject = GetComponent<NetworkObject>();
                    if (networkObject != null) networkObject.Despawn();
                }
            }

            // do ap leech
            if (damageDealer != null && damageDealerNOID > 0)
            {
                NetworkObject damageDealerNetworkObject = damageDealer.GetComponent<NetworkObject>();
                if (damageDealerNetworkObject != null)
                {
                    NetworkCharacter nc_damageDealer = damageDealerNetworkObject.GetComponent<NetworkCharacter>();
                    if (nc_damageDealer != null)
                    {
                        nc_damageDealer.ApCurrent.Value += (int)(damage * nc_damageDealer.ApLeech.Value);
                    }
                }
            }
        }
    }

    [ClientRpc]
    private void HandleEnemyTakeDamageClientRpc(float damage, bool isCritical, ulong damageDealerNOID = 0)
    {
        // get the local player
        ulong localPlayerNOID = 0;
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.GetComponent<NetworkObject>().IsLocalPlayer)
            {
                localPlayerNOID = player.GetComponent<NetworkObject>().NetworkObjectId;
            }
        }

        // LOCAL PLAYER
        if (IsClient)
        {
            if (localPlayerNOID != damageDealerNOID)
            {
                // do sprite flash
                SpriteFlash spriteFlash = GetComponentInChildren<SpriteFlash>();
                spriteFlash?.DamageFlash();
            }
        }
    }

    // PLAYER
    public void HandlePlayerTakeDamage(float damage, bool isCritical, ulong damageDealerNOID = 0)
    {
        var playerController = GetComponent<PlayerController>();
        if (playerController == null) return;

        // SERVER or HOST
        if (IsServer)
        {
            if (playerController.IsInvulnerable)
            {
                return;
            }

            damage = ApplyShieldToDamage(damage);
            if (damage <= 0)
            {
                return;
            }

            if (!IsHost)
            {
                HandlePlayerTakeDamageClientRpc(damage, isCritical, damageDealerNOID);
            }

            HpCurrent.Value -= (int)damage;
            if (HpCurrent.Value < 0) { HpCurrent.Value = 0; }
            DamagePopupTextClientRpc(damage, isCritical);

            if (HpCurrent.Value <= 0)
            {
                HpCurrent.Value = 0;
                Debug.Log("Hp hit 0, KillPlayer");
                playerController.KillPlayer(REKTCanvas.TypeOfREKT.HP);
            }

            // do ap leech
            if (damageDealerNOID > 0)
            {
                var damageDealer = NetworkManager.SpawnManager.SpawnedObjects[damageDealerNOID];
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

        // CLIENT or HOST
        if (IsClient)
        {
            // do sprite flash
            SpriteFlash spriteFlash = GetComponentInChildren<SpriteFlash>();
            spriteFlash?.DamageFlash();

            // do local only effects
            if (gameObject.GetComponent<NetworkObject>().IsLocalPlayer)
            {
                BloodBorderCanvas.Instance.DoBlood();
                gameObject.GetComponent<PlayerCamera>().Shake(1.5f, 0.3f);
            }
        }
    }

    private float ApplyShieldToDamage(float damage)
    {
        PlayerAbilities playerAbilities = GetComponent<PlayerAbilities>();
        if (playerAbilities.shieldBlock != null)
        {
            ShieldBlock shieldBlock = playerAbilities.shieldBlock.GetComponent<ShieldBlock>();
            if (shieldBlock.IsBlocking())
            {
                damage = shieldBlock.AbsorbDamage(damage);
            }
        }
        return damage;
    }

    [ClientRpc]
    private void HandlePlayerTakeDamageClientRpc(float damage, bool isCritical, ulong damageDealerNOID = 0)
    {
        HandlePlayerTakeDamage(damage, isCritical, damageDealerNOID);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DamagePopupTextClientRpc(float damage, bool isCritical)
    {
        //ColorUtility.TryParseHtmlString(hexColorStr, out Color color);

        PopupTextManager.Instance.PopupText(
            damage.ToString("F0"),
            transform.position + k_popupTextOffset,
            isCritical ? ReceiveCriticalDamageFontSize : ReceiveDamageFontSize,
            isCritical ? ReceiveCriticalDamageColor : ReceiveDamageColor,
            0.2f);
    }

    protected virtual void InitializeStats()
    {
        // set values to base values
        HpMax.Value = baseHpMax;           // needs to be += as we also add to hp from enemy controller with dynamicHp
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
        ApRegen.Value = baseApRegen;
        KnockbackMultiplier.Value = baseKnockbackMutliplier;
        StunMultiplier.Value = baseStunMultiplier;

        // check for and apply dynamic HP
        DynamicHP dynamicHp = GetComponent<DynamicHP>();
        dynamicHp?.ApplyDynamicHp();
    }

    public bool HasBuffObject(BuffObject buffObject)
    {
        return activeBuffObjects.Contains(buffObject);
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
            RecalculateStats(); // Recalculate stats after adding a new buff
            AddBuffObjectNameClientRpc(buffObject.name);
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
            RecalculateStats(); // Recalculate stats after removing a buff
            RemoveBuffObjectNameClientRpc(buffObject.name);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AddBuffObjectNameClientRpc(string name)
    {
        if (!activeBuffNames_CLIENT.Contains(name))
        {
            activeBuffNames_CLIENT.Add(name);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RemoveBuffObjectNameClientRpc(string name)
    {
        if (activeBuffNames_CLIENT.Contains(name))
        {
            activeBuffNames_CLIENT.Remove(name);
        }
    }

    public bool HasBuffName(string name)
    {
        return activeBuffNames_CLIENT.Contains(name);
    }

    public void RecalculateStats()
    {
        if (!IsServer)
        {
            Debug.LogError("Cannot call CalculateFinalStats() from client");
            return;
        }

        var hpRatio = HpCurrent.Value / HpMax.Value;
        var apRatio = ApCurrent.Value / ApMax.Value;

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
            { CharacterStat.ApRegen, baseApRegen }
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
        HpMax.Value = baseStats[CharacterStat.HpMax];
        HpCurrent.Value = HpMax.Value * hpRatio;
        HpBuffer.Value = baseStats[CharacterStat.HpBuffer];
        AttackPower.Value = baseStats[CharacterStat.AttackPower];
        CriticalChance.Value = baseStats[CharacterStat.CriticalChance];
        ApMax.Value = baseStats[CharacterStat.ApMax];
        ApCurrent.Value = ApMax.Value * apRatio;
        ApBuffer.Value = baseStats[CharacterStat.ApBuffer];
        DoubleStrikeChance.Value = baseStats[CharacterStat.DoubleStrikeChance];
        CriticalDamage.Value = baseStats[CharacterStat.CriticalDamage];
        MoveSpeed.Value = baseStats[CharacterStat.MoveSpeed];
        Accuracy.Value = baseStats[CharacterStat.Accuracy];
        Evasion.Value = baseStats[CharacterStat.Evasion];
        DamageReduction.Value = baseStats[CharacterStat.DamageReduction];
        ApLeech.Value = baseStats[CharacterStat.ApLeech];
        ApRegen.Value = baseStats[CharacterStat.ApRegen];

        // Optionally, you can log the final stats for debugging
        //Debug.Log("Player Stats Updated");
    }

    public void AddHp(int amount)
    {
        HpCurrent.Value += amount;
        if (HpCurrent.Value > HpMax.Value)
        {
            HpCurrent.Value = HpMax.Value;
        }
    }
}