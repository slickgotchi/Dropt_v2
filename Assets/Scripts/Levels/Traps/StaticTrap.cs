namespace Level.Traps
{
    public sealed class StaticTrap : DamagedTrap
    {
        protected override bool IsAvailableForAttack => true;

        protected override void ActivateDamage()
        {
            if (null == m_group)
                return;

            for (int i = m_group.Traps.Count - 1; i >= 0; i--)
            {
                if (!(m_group.Traps[i] is StaticTrap))
                    continue;

                var trap = m_group.Traps[i] as StaticTrap;
                if (trap == this || trap.Group.Value != Group.Value)
                    continue;

                trap.ResetCooldown();
            }
        }

        private void ResetCooldown()
        {
            m_cooldownTimer = m_cooldownDuration;
        }
    }
}