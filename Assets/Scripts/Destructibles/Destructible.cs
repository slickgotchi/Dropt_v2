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

    private NetworkVariable<int> CurrentHp;

    private SoundFX_Destructible m_soundFX_Destructible;

    private void Awake()
    {
        CurrentHp = new NetworkVariable<int>(Hp);
        m_soundFX_Destructible = GetComponent<SoundFX_Destructible>();
    }

    //public override void OnNetworkSpawn()
    //{
    //base.OnNetworkSpawn();

    // configure audio
    //if (type == Type.Crafted) audioOnHit = AudioLibrary.Instance.HitCrafted;
    //if (type == Type.Inorganic) audioOnHit = AudioLibrary.Instance.HitInorganic;
    //if (type == Type.Organic) audioOnHit = AudioLibrary.Instance.HitOrganic;
    //}

    public void Explode()
    {
        TakeDamage(CurrentHp.Value);
    }

    public void TakeDamage(Wearable.WeaponTypeEnum weaponType)
    {
        var damage = CalculateDamageToDestructible(type, weaponType);
        m_soundFX_Destructible.PlayTakeDamageSound();

        if (CurrentHp.Value <= damage)
        {
            VisualEffectsManager.Singleton.SpawnCloudExplosion(transform.position + new Vector3(0, 0.5f, 0));
        }

        if (IsServer)
        {
            CurrentHp.Value -= damage;
            if (CurrentHp.Value <= 0)
            {
                PRE_DIE?.Invoke();
                GetComponent<NetworkObject>().Despawn();
                DIE?.Invoke();
            }
        }

        DamageWobble damageWobble = GetComponent<DamageWobble>();
        damageWobble?.Play();
    }

    public void TakeDamage(int damage)
    {
        if (IsServer)
        {
            CurrentHp.Value -= damage;
            if (IsServer && CurrentHp.Value <= 0)
            {
                GetComponent<NetworkObject>().Despawn();
            }
        }

        DamageWobble damageWobble = GetComponent<DamageWobble>();
        damageWobble?.Play();

        if (CurrentHp.Value <= damage)
        {
            VisualEffectsManager.Singleton.SpawnCloudExplosion(transform.position + new Vector3(0, 0.5f, 0));
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
            if (weaponType == Wearable.WeaponTypeEnum.Ballistic) damage = 2;
            if (weaponType == Wearable.WeaponTypeEnum.Splash) damage = 3;
            if (weaponType == Wearable.WeaponTypeEnum.Magic) damage = 2;
            if (weaponType == Wearable.WeaponTypeEnum.Shield) damage = 2;
        }

        if (destructibleType == Type.Inorganic)
        {
            if (weaponType == Wearable.WeaponTypeEnum.Cleave) damage = 2;
            if (weaponType == Wearable.WeaponTypeEnum.Smash) damage = 3;
            if (weaponType == Wearable.WeaponTypeEnum.Pierce) damage = 2;
            if (weaponType == Wearable.WeaponTypeEnum.Unarmed) damage = 2;
            if (weaponType == Wearable.WeaponTypeEnum.Ballistic) damage = 2;
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

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        m_soundFX_Destructible.PlayDieSound();
    }
}