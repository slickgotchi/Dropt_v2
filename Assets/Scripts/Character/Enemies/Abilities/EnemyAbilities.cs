using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyAbilities : MonoBehaviour
{
    [Header("Ability Prefabs")]
    public GameObject PrimaryAttack;
    public GameObject SecondaryAttack;

    private float m_cooldownTimer = 0;
    private float m_cooldownDuration = 0;

    public bool IsAbilityRunning() { 
        // cooldown timer includes telegraph, execution and cooldown durations
        // we consider the ability running if we are in the telegraph and execution phases
        return m_cooldownTimer > m_cooldownDuration; 
    }

    private void Update()
    {
        m_cooldownTimer -= Time.deltaTime;
    }

    public bool TryActivate(GameObject abilityPrefab, GameObject parent, GameObject target)
    {
        if (m_cooldownTimer > 0) return false;

        if (abilityPrefab == null)
        {
            Debug.LogWarning("TryActivate(): Need to pass a valid abilityPrefab");
            return false;
        }

        var enemyAbility = abilityPrefab.GetComponent<EnemyAbility>();
        if (enemyAbility == null)
        {
            Debug.LogWarning("TryActivate(): Enemy ability not assigned yet");
            return false;
        }
        m_cooldownTimer = enemyAbility.TelegraphDuration + enemyAbility.ExecutionDuration + enemyAbility.CooldownDuration;
        m_cooldownDuration = enemyAbility.CooldownDuration;

        var no_ability = Instantiate(abilityPrefab);
        no_ability.GetComponent<EnemyAbility>().Target = target;
        no_ability.GetComponent<EnemyAbility>().Parent = parent;
        no_ability.GetComponent<NetworkObject>().Spawn();
        no_ability.GetComponent<EnemyAbility>().Activate();

        return true;
    }


}
