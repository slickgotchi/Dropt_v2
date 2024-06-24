using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Destructible : NetworkBehaviour
{
    public enum Type { Organic, Inorganic, Crafted }
    public Type type;
    public int Hp = 1;

    public void TakeDamage(Wearable.WeaponTypeEnum weaponType)
    {
        if (IsServer)
        {
            Hp -= CalculateDamageToDestructible(type, weaponType);
            if (Hp <= 0)
            {
                GetComponent<NetworkObject>().Despawn();
            }
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
