using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Destructible : NetworkBehaviour
{
    public enum Type { Organic, Inorganic, Crafted }
    public Type type;
    public int Hp = 1;

    private NetworkVariable<int> CurrentHp;

    void Awake()
    {
        CurrentHp = new NetworkVariable<int>(Hp);
    }

    public void TakeDamage(Wearable.WeaponTypeEnum weaponType)
    {
        var damage = CalculateDamageToDestructible(type, weaponType);

        if (CurrentHp.Value <= damage)
        {
            VisualEffectsManager.Singleton.SpawnCloudExplosion(transform.position + new Vector3(0, 0.5f, 0));
        }

        if (IsServer)
        {
            CurrentHp.Value -= damage;
            if (CurrentHp.Value <= 0)
            {
                if (IsServer) GetComponent<NetworkObject>().Despawn();
            }
        }

        var damageWobble = GetComponent<DamageWobble>();
        if (damageWobble != null) damageWobble.Play();

    }

    public void TakeDamage(int damage)
    {
        if (IsServer)
        {
            CurrentHp.Value -= damage;
            if (CurrentHp.Value <= 0)
            {
                if (IsServer) GetComponent<NetworkObject>().Despawn();
            }
        }

        var damageWobble = GetComponent<DamageWobble>();
        if (damageWobble != null) damageWobble.Play();

        if (CurrentHp.Value <= damage)
        {
            VisualEffectsManager.Singleton.SpawnCloudExplosion(transform.position + new Vector3(0, 0.5f, 0));
        }
    }

    int CalculateDamageToDestructible(Type destructibleType, Wearable.WeaponTypeEnum weaponType)
    {
        int damage = 1;

        // do comparison matrix
        if (destructibleType == Type.Organic)
        {
            if (weaponType == Wearable.WeaponTypeEnum.Cleave) damage = 3;
            if (weaponType == Wearable.WeaponTypeEnum.Smash) damage = 1;
            if (weaponType == Wearable.WeaponTypeEnum.Pierce) damage = 2;
            if (weaponType == Wearable.WeaponTypeEnum.Unarmed) damage = 1;
            if (weaponType == Wearable.WeaponTypeEnum.Ballistic) damage = 1;
            if (weaponType == Wearable.WeaponTypeEnum.Splash) damage = 3;
            if (weaponType == Wearable.WeaponTypeEnum.Magic) damage = 2;
            if (weaponType == Wearable.WeaponTypeEnum.Shield) damage = 2;
        }
        if (destructibleType == Type.Inorganic)
        {
            if (weaponType == Wearable.WeaponTypeEnum.Cleave) damage = 2;
            if (weaponType == Wearable.WeaponTypeEnum.Smash) damage = 3;
            if (weaponType == Wearable.WeaponTypeEnum.Pierce) damage = 1;
            if (weaponType == Wearable.WeaponTypeEnum.Unarmed) damage = 1;
            if (weaponType == Wearable.WeaponTypeEnum.Ballistic) damage = 2;
            if (weaponType == Wearable.WeaponTypeEnum.Splash) damage = 1;
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
            if (weaponType == Wearable.WeaponTypeEnum.Splash) damage = 2;
            if (weaponType == Wearable.WeaponTypeEnum.Magic) damage = 1;
            if (weaponType == Wearable.WeaponTypeEnum.Shield) damage = 2;
        }

        return damage;
    }
}
