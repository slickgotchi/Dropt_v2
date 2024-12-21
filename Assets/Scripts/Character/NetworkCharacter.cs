using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System;
using Unity.Mathematics;

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

    public DynamicStats previousDynamicStats;
    public DynamicStats currentDynamicStats;
    public StaticStats previousStaticStats;
    public StaticStats currentStaticStats;

    [Serializable]
    public struct DynamicStats : INetworkSerializable
    {
        public float HpCurrent;
        public float ApCurrent;
        public float EnemyShield;
        public bool IsDead;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref HpCurrent);
            serializer.SerializeValue(ref ApCurrent);
            serializer.SerializeValue(ref EnemyShield);
            serializer.SerializeValue(ref IsDead);
        }
    }

    [Serializable]
    public struct StaticStats : INetworkSerializable
    {
        public float HpMax;
        public float HpBuffer;
        public float AttackPower;
        public float CriticalChance;
        public float ApMax;
        public float ApBuffer;
        public float DoubleStrikeChance;
        public float CriticalDamage;

        public float MoveSpeed;
        public float Accuracy;
        public float Evasion;
        public float DamageReduction;

        public float ApLeech;
        public float ApRegen;
        public float KnockbackMultiplier;
        public float StunMultiplier;
        public float MaxEnemyShield;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            // Integers
            serializer.SerializeValue(ref HpMax);
            serializer.SerializeValue(ref HpBuffer);
            serializer.SerializeValue(ref AttackPower);
            serializer.SerializeValue(ref CriticalChance);
            serializer.SerializeValue(ref ApMax);
            serializer.SerializeValue(ref ApBuffer);
            serializer.SerializeValue(ref DoubleStrikeChance);
            serializer.SerializeValue(ref CriticalDamage);

            serializer.SerializeValue(ref MoveSpeed);
            serializer.SerializeValue(ref Accuracy);
            serializer.SerializeValue(ref Evasion);
            serializer.SerializeValue(ref DamageReduction);

            serializer.SerializeValue(ref ApLeech);
            serializer.SerializeValue(ref ApRegen);
            serializer.SerializeValue(ref KnockbackMultiplier);
            serializer.SerializeValue(ref StunMultiplier);
            serializer.SerializeValue(ref MaxEnemyShield);
        }
    }

    private float k_floatTolerance = 0.01f;

    /// <summary>
    /// Compares two DynamicStatData structs and returns true if they are equal.
    /// </summary>
    private bool DynamicStatsAreEqual(DynamicStats current, DynamicStats previous)
    {
        return math.abs(current.HpCurrent - previous.HpCurrent) < k_floatTolerance &&
               math.abs(current.ApCurrent - previous.ApCurrent) < k_floatTolerance &&
               math.abs(current.EnemyShield - previous.EnemyShield) < k_floatTolerance &&
               current.IsDead == previous.IsDead;
    }

    /// <summary>
    /// Compares two StaticStatData structs and returns true if they are equal.
    /// </summary>
    private bool StaticStatsAreEqual(StaticStats current, StaticStats previous)
    {
        return math.abs(current.HpMax - previous.HpMax) < k_floatTolerance &&
               math.abs(current.HpBuffer - previous.HpBuffer) < k_floatTolerance &&
               math.abs(current.AttackPower - previous.AttackPower) < k_floatTolerance &&
               math.abs(current.CriticalChance - previous.CriticalChance) < k_floatTolerance &&
               math.abs(current.ApMax - previous.ApMax) < k_floatTolerance &&
               math.abs(current.ApBuffer - previous.ApBuffer) < k_floatTolerance &&
               math.abs(current.DoubleStrikeChance - previous.DoubleStrikeChance) < k_floatTolerance &&
               math.abs(current.CriticalDamage - previous.CriticalDamage) < k_floatTolerance &&
               math.abs(current.MoveSpeed - previous.MoveSpeed) < k_floatTolerance &&
               math.abs(current.Accuracy - previous.Accuracy) < k_floatTolerance &&
               math.abs(current.Evasion - previous.Evasion) < k_floatTolerance &&
               math.abs(current.DamageReduction - previous.DamageReduction) < k_floatTolerance &&
               math.abs(current.ApLeech - previous.ApLeech) < k_floatTolerance &&
               math.abs(current.ApRegen - previous.ApRegen) < k_floatTolerance &&
               math.abs(current.KnockbackMultiplier - previous.KnockbackMultiplier) < k_floatTolerance &&
               math.abs(current.StunMultiplier - previous.StunMultiplier) < k_floatTolerance &&
               math.abs(current.MaxEnemyShield - previous.MaxEnemyShield) < k_floatTolerance;
    }

    void SyncStats()
    {
        if (!IsServer) return;

        if (!DynamicStatsAreEqual(previousDynamicStats, currentDynamicStats))
        {
            previousDynamicStats = currentDynamicStats;
            SyncDynamicStatsClientRpc(currentDynamicStats);
            //Debug.Log("Update Dynamic Stats");
        }

        if (!StaticStatsAreEqual(previousStaticStats, currentStaticStats))
        {
            previousStaticStats = currentStaticStats;
            SyncStaticStatsClientRpc(currentStaticStats);
            //Debug.Log("Update Static Stats");
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SyncDynamicStatsClientRpc(DynamicStats newDynamicStats)
    {
        currentDynamicStats = newDynamicStats;
        previousDynamicStats = newDynamicStats;
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SyncStaticStatsClientRpc(StaticStats newStaticStats)
    {
        currentStaticStats = newStaticStats;
        previousStaticStats = newStaticStats;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            // baseize default values on the server
            InitializeStats();
        }
    }


    private float m_syncTimer = 0f;
    private float k_syncInterval = 0.1f;

    protected virtual void Update()
    {
        if (!IsServer) return;

        // handle ap regen
        currentDynamicStats.ApCurrent += (int)(currentStaticStats.ApRegen * Time.deltaTime);
        if (currentDynamicStats.ApCurrent > currentStaticStats.ApMax)
        {
            currentDynamicStats.ApCurrent = currentStaticStats.ApMax;
        }

        // handle stat syncing
        m_syncTimer -= Time.deltaTime;
        if (m_syncTimer < 0)
        {
            m_syncTimer = k_syncInterval;
            SyncStats();
        }
    }

    public float GetAttackPower(float randomVariation = 0.1f)
    {
        return UnityEngine.Random.Range(
            currentStaticStats.AttackPower * (1 - randomVariation),
            currentStaticStats.AttackPower * (1 + randomVariation));
    }

    public bool IsCriticalAttack()
    {
        var rand = UnityEngine.Random.Range(0f, 0.999f);
        return rand < currentStaticStats.CriticalChance;
    }

    public virtual void TakeDamage(float damage, bool isCritical, GameObject damageDealer = null)
    {
        // reduce damage
        damage *= (1 - currentStaticStats.DamageReduction);

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
            if (currentDynamicStats.EnemyShield > 0)
            {
                currentDynamicStats.EnemyShield--;
                damage = 0;
            }
            if (!IsHost)
            {
                HandleEnemyTakeDamageClientRpc(damage, isCritical, damageDealerNOID);
            }

            // deplete hp
            currentDynamicStats.HpCurrent -= (int)damage;
            if (currentDynamicStats.HpCurrent < 0) { currentDynamicStats.HpCurrent = 0; }

            // show damage text
            DamagePopupTextClientRpc(damage, isCritical);

            // check for death
            if (currentDynamicStats.HpCurrent <= 0 && enemyController != null)
            {
                currentDynamicStats.IsDead = true;

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
                    PlayEnemyDieSoundClientRpc();
                    var networkObject = GetComponent<NetworkObject>();
                    if (networkObject != null) networkObject.Despawn();

                    Debug.Log("HandleEnemyTakeDamage: ReturnNetworkObject()");
                    Core.Pool.NetworkObjectPool.Instance.ReturnNetworkObject(
                        networkObject, networkObject.gameObject);
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
                        nc_damageDealer.currentDynamicStats.ApCurrent += (int)(damage * nc_damageDealer.currentStaticStats.ApLeech);
                    }
                }
            }
        }
    }

    [ClientRpc]
    public void PlayEnemyDieSoundClientRpc()
    {
        PlayEnemyDieSound();
    }

    public virtual void PlayEnemyDieSound() { }

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

            currentDynamicStats.HpCurrent -= (int)damage;
            if (currentDynamicStats.HpCurrent < 0) { currentDynamicStats.HpCurrent = 0; }
            DamagePopupTextClientRpc(damage, isCritical);

            if (currentDynamicStats.HpCurrent <= 0)
            {
//<<<<<<< HEAD
                currentDynamicStats.HpCurrent = 0;
                //Debug.Log("Hp hit 0, KillPlayer");
//=======
//                HpCurrent.Value = 0;
//                //Debug.Log("Hp hit 0, KillPlayer");
//>>>>>>> main
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
                        nc_damageDealer.currentDynamicStats.ApCurrent += (int)(damage * nc_damageDealer.currentStaticStats.ApLeech);
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
        currentDynamicStats.HpCurrent = baseHpCurrent;
        currentDynamicStats.ApCurrent = baseApCurrent;

        currentStaticStats.HpMax = baseHpMax;           // needs to be += as we also add to hp from enemy controller with dynamicHp
        currentStaticStats.HpBuffer = baseHpBuffer;
        currentStaticStats.AttackPower = baseAttackPower;
        currentStaticStats.CriticalChance = baseCriticalChance;
        currentStaticStats.ApMax = baseApMax;
        currentStaticStats.ApBuffer = baseApBuffer;
        currentStaticStats.DoubleStrikeChance = baseDoubleStrikeChance;
        currentStaticStats.CriticalDamage = baseCriticalDamage;
        currentStaticStats.MoveSpeed = baseMoveSpeed;
        currentStaticStats.Accuracy = baseAccuracy;
        currentStaticStats.Evasion = baseEvasion;
        currentStaticStats.DamageReduction = baseDamageReduction;
        currentStaticStats.ApLeech = baseApLeech;
        currentStaticStats.ApRegen = baseApRegen;
        currentStaticStats.KnockbackMultiplier = baseKnockbackMutliplier;
        currentStaticStats.StunMultiplier = baseStunMultiplier;

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

        var hpRatio = currentDynamicStats.HpCurrent / currentStaticStats.HpMax;
        var apRatio = currentDynamicStats.ApCurrent / currentStaticStats.ApMax;

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

        currentStaticStats.HpMax = baseStats[CharacterStat.HpMax];
        currentStaticStats.HpBuffer = baseStats[CharacterStat.HpBuffer];
        currentStaticStats.AttackPower = baseStats[CharacterStat.AttackPower];
        currentStaticStats.CriticalChance = baseStats[CharacterStat.CriticalChance];
        currentStaticStats.ApMax = baseStats[CharacterStat.ApMax];
        currentStaticStats.ApBuffer = baseStats[CharacterStat.ApBuffer];
        currentStaticStats.DoubleStrikeChance = baseStats[CharacterStat.DoubleStrikeChance];
        currentStaticStats.CriticalDamage = baseStats[CharacterStat.CriticalDamage];
        currentStaticStats.MoveSpeed = baseStats[CharacterStat.MoveSpeed];
        currentStaticStats.Accuracy = baseStats[CharacterStat.Accuracy];
        currentStaticStats.Evasion = baseStats[CharacterStat.Evasion];
        currentStaticStats.DamageReduction = baseStats[CharacterStat.DamageReduction];
        currentStaticStats.ApLeech = baseStats[CharacterStat.ApLeech];
        currentStaticStats.ApRegen = baseStats[CharacterStat.ApRegen];

        currentDynamicStats.HpCurrent = currentStaticStats.HpMax * hpRatio;
        currentDynamicStats.ApCurrent = currentStaticStats.ApMax * apRatio;
        // Optionally, you can log the final stats for debugging
        //Debug.Log("Player Stats Updated");
    }

    public void AddHp(int amount)
    {
        currentDynamicStats.HpCurrent += amount;
        if (currentDynamicStats.HpCurrent > currentStaticStats.HpMax)
        {
            currentDynamicStats.HpCurrent = currentStaticStats.HpMax;
        }
    }
}