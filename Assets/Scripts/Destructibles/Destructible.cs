using System;
using Unity.Netcode;
using UnityEngine;

public class Destructible : NetworkBehaviour
{
    public event Action DIE;
    public event Action PRE_DIE;

    //public AudioClip audioOnHit;

    public enum Type
    {
        Organic,
        Inorganic,
        Crafted
    }

    public Type type;
    public int Hp = 1;

    private int previousHp;
    private int currentHp;

    private SoundFX_Destructible m_soundFX_Destructible;

    private void Awake()
    {
        previousHp = Hp;
        currentHp = Hp;
        m_soundFX_Destructible = GetComponent<SoundFX_Destructible>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        // remove any level spawn components
        LevelSpawnManager.Instance.RemoveLevelSpawnComponent(GetComponent<Level.LevelSpawn>());

        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (IsServer)
        {
            if (previousHp != currentHp)
            {
                previousHp = currentHp;
                SyncHpClientRpc(currentHp);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SyncHpClientRpc(int hp)
    {
        previousHp = hp;
        currentHp = hp;
    }

    public void Explode(ulong damageDealerId)
    {
        DoDamage(currentHp, damageDealerId);
    }

    public void TakeDamage(Wearable.WeaponTypeEnum weaponType, ulong damageDealerId)
    {
        var damage = CalculateDamageToDestructible(type, weaponType);
        DoDamage(damage, damageDealerId);
    }

    private void DoDamage(int damage, ulong damageDealerId)
    {
        // do client actions
        if (IsClient)
        {
            if (currentHp <= damage)
            {
                // play cloud explosion
                VisualEffectsManager.Instance.SpawnCloudExplosion(transform.position + new Vector3(0, 0.5f, 0));

                // hide all sprites
                var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
                foreach (var sr in spriteRenderers) sr.enabled = false;

                // disable all colliders
                var colliders = GetComponentsInChildren<Collider2D>();
                foreach (var c in colliders) c.enabled = false;
            }
            else
            {
                // do damage wobble
                DamageWobble damageWobble = GetComponent<DamageWobble>();
                if (damageWobble != null) damageWobble?.Play();

                // play hit sfx
                m_soundFX_Destructible.PlayTakeDamageSound();
            }
        }

        // do server check for despawn
        if (IsServer)
        {
            currentHp -= damage;
            if (currentHp <= 0)
            {
                PRE_DIE?.Invoke();
                NotifyPlayerToDestroyDestructible(damageDealerId);
                DestroyDestructibleSoundClientRpc();

                var levelSpawn = GetComponent<Level.LevelSpawn>();

                //Core.Pool.NetworkObjectPool.Instance.ReturnNetworkObject(
                //    GetComponent<NetworkObject>(), levelSpawn.prefab);
                //GetComponent<NetworkObject>().Despawn(false);
                GetComponent<NetworkObject>().Despawn();
                DIE?.Invoke();
            }
        }
    }

    private int CalculateDamageToDestructible(Type destructibleType, Wearable.WeaponTypeEnum weaponType)
    {
        int damage = 1;

        // do comparison matrix
        if (destructibleType == Type.Organic)
        {
            if (weaponType == Wearable.WeaponTypeEnum.Cleave) damage = 3;
            if (weaponType == Wearable.WeaponTypeEnum.Smash) damage = 1;
            if (weaponType == Wearable.WeaponTypeEnum.Pierce) damage = 2;
            if (weaponType == Wearable.WeaponTypeEnum.Unarmed) damage = 1;
            if (weaponType == Wearable.WeaponTypeEnum.Ballistic) damage = 3;
            if (weaponType == Wearable.WeaponTypeEnum.Splash) damage = 3;
            if (weaponType == Wearable.WeaponTypeEnum.Magic) damage = 2;
            if (weaponType == Wearable.WeaponTypeEnum.Shield) damage = 2;
        }

        if (destructibleType == Type.Inorganic)
        {
            if (weaponType == Wearable.WeaponTypeEnum.Cleave) damage = 2;
            if (weaponType == Wearable.WeaponTypeEnum.Smash) damage = 3;
            if (weaponType == Wearable.WeaponTypeEnum.Pierce) damage = 3;
            if (weaponType == Wearable.WeaponTypeEnum.Unarmed) damage = 2;
            if (weaponType == Wearable.WeaponTypeEnum.Ballistic) damage = 3;
            if (weaponType == Wearable.WeaponTypeEnum.Splash) damage = 2;
            if (weaponType == Wearable.WeaponTypeEnum.Magic) damage = 3;
            if (weaponType == Wearable.WeaponTypeEnum.Shield) damage = 2;
        }

        if (destructibleType == Type.Crafted)
        {
            if (weaponType == Wearable.WeaponTypeEnum.Cleave) damage = 1;
            if (weaponType == Wearable.WeaponTypeEnum.Smash) damage = 2;
            if (weaponType == Wearable.WeaponTypeEnum.Pierce) damage = 3;
            if (weaponType == Wearable.WeaponTypeEnum.Unarmed) damage = 1;
            if (weaponType == Wearable.WeaponTypeEnum.Ballistic) damage = 3;
            if (weaponType == Wearable.WeaponTypeEnum.Splash) damage = 1;
            if (weaponType == Wearable.WeaponTypeEnum.Magic) damage = 2;
            if (weaponType == Wearable.WeaponTypeEnum.Shield) damage = 2;
        }

        return damage;
    }

    private void NotifyPlayerToDestroyDestructible(ulong id)
    {
        NetworkObject networkObject = NetworkManager.SpawnManager.SpawnedObjects[id];
        PlayerController playerController = networkObject.GetComponent<PlayerController>();
        playerController?.DestroyDestructible();
    }

    [ClientRpc]
    private void DestroyDestructibleSoundClientRpc()
    {
        m_soundFX_Destructible.PlayDieSound();
    }
}