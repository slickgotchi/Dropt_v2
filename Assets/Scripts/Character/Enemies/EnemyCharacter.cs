public class EnemyCharacter : NetworkCharacter
{
    public override void OnNetworkSpawn()
    {
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
}