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
        HpMax.Value *= CodeInjector.Instance.EnemyHp.GetValue();
        HpCurrent.Value *= CodeInjector.Instance.EnemyHp.GetValue();
        AttackPower.Value *= CodeInjector.Instance.EnemyDamage.GetValue();
        MoveSpeed.Value *= CodeInjector.Instance.EnemySpeed.GetValue();
        EnemyShield.Value = CodeInjector.Instance.EnemyShield.GetValue();
        MaxEnemyShield.Value = CodeInjector.Instance.EnemyShield.GetValue();
    }

    public override void TakeDamage(float damage, bool isCritical, GameObject damageDealer = null)
    {
        base.TakeDamage(damage, isCritical, damageDealer);
        m_soundFX_Enemy?.PlayTakeDamageSound();
    }
}