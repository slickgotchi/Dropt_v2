namespace Level.Traps
{
    public sealed class StaticTrap : DamagedTrap
    {
        protected override bool IsAvailableForAttack => true;
    }
}