using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Mathematics;

public class DynamicHP : NetworkBehaviour
{
    public float Multiplier = 1f;

    public void ApplyDynamicHp()
    {
        if (!IsServer) return;

        var playerCharacters = FindObjectsByType<PlayerCharacter>(FindObjectsSortMode.None);
        var netAttackPowers = new List<float>();
        foreach (var pc in playerCharacters)
        {
            netAttackPowers.Add(GetPlayerNetAttackPower(pc));
        }

        netAttackPowers.Sort((a, b) => b.CompareTo(a));

        // now assign dynamic HP
        float dynamicHp = 0;
        if (netAttackPowers.Count == 1)
        {
            dynamicHp = netAttackPowers[0];
        }
        else if (netAttackPowers.Count == 2)
        {
            dynamicHp = netAttackPowers[0] + netAttackPowers[1] * 0.5f;
        }
        else if (netAttackPowers.Count == 3)
        {
            dynamicHp = netAttackPowers[0] + netAttackPowers[1] * 0.5f + netAttackPowers[2] * 0.25f;
        }

        dynamicHp *= Multiplier;

        GetComponent<NetworkCharacter>().currentStaticStats.HpMax += dynamicHp;
        GetComponent<NetworkCharacter>().currentDynamicStats.HpCurrent += dynamicHp;
    }

    float GetPlayerNetAttackPower(PlayerCharacter playerCharacter)
    {
        var playerEquipment = playerCharacter.GetComponent<PlayerEquipment>();
        var lhWearableNameEnum = playerEquipment.LeftHand.Value;
        var rhWearableNameEnum = playerEquipment.RightHand.Value;

        var lhWearable = WearableManager.Instance.GetWearable(lhWearableNameEnum);
        var rhWearable = WearableManager.Instance.GetWearable(rhWearableNameEnum);

        float lhRarityMultiplier = lhWearable == null ? 1 : lhWearable.RarityMultiplier;
        float rhRarityMultiplier = rhWearable == null ? 1 : rhWearable.RarityMultiplier;

        float rarityMultiplier = math.max(lhRarityMultiplier, rhRarityMultiplier);

        var baseAttack = playerCharacter.currentStaticStats.AttackPower * rarityMultiplier;
        var baseCrit = playerCharacter.currentStaticStats.CriticalChance;

        float netAttackPower = (baseAttack * (1 - baseCrit) + (baseAttack * 2 * baseCrit));

        return netAttackPower;
    }
}
