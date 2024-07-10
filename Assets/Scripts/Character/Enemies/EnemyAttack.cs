using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public GameObject AttackPrefab;
    public float TelegraphDuration = 1f;
    public float AttackDuration = 1f;
    public float CooldownDuration = 2f;

    private float m_cooldownTimer = 0;

    public bool IsCooldownFinished()
    {
        return m_cooldownTimer <= 0;
    }

    public void ResetCooldown()
    {
        m_cooldownTimer = CooldownDuration;
    }

    private void Update()
    {
        m_cooldownTimer -= Time.deltaTime;
    }
}
