using UnityEngine;

public class EnemyCharacter : NetworkCharacter
{
    private SoundFX_Enemy m_soundFX_Enemy;

    public override void OnNetworkSpawn()
    {
        m_soundFX_Enemy = GetComponent<SoundFX_Enemy>();
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            return;
        }

        LinkCodeInjectorMultipliers();
    }

    private void LinkCodeInjectorMultipliers()
    {
        currentStaticStats.HpMax *= CodeInjector.Instance.EnemyHp.GetValue();
        currentDynamicStats.HpCurrent *= CodeInjector.Instance.EnemyHp.GetValue();
        currentStaticStats.AttackPower *= CodeInjector.Instance.EnemyDamage.GetValue();
        currentStaticStats.MoveSpeed *= CodeInjector.Instance.EnemySpeed.GetValue();
        currentDynamicStats.EnemyShield = CodeInjector.Instance.EnemyShield.GetValue();
        currentStaticStats.MaxEnemyShield = CodeInjector.Instance.EnemyShield.GetValue();
    }

    public override void TakeDamage(float damage, bool isCritical, GameObject damageDealer = null)
    {
        base.TakeDamage(damage, isCritical, damageDealer);
        if (currentDynamicStats.HpCurrent > 0)
        {
            m_soundFX_Enemy?.PlayTakeDamageSound();
            return;
        }
    }

    public override void PlayEnemyDieSound()
    {
        base.PlayEnemyDieSound();
        m_soundFX_Enemy.PlayDieSound();
    }
}