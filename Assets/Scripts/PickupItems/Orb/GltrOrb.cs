namespace PickupItems.Orb
{
    public class GltrOrb : BaseOrb
    {
        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                Init(m_size);
            }
        }

        protected override int CalculateValue(PickupItemManager.Size size)
        {
            return size switch
            {
                PickupItemManager.Size.Tiny => 1,
                PickupItemManager.Size.Small => 5,
                PickupItemManager.Size.Medium => 25,
                PickupItemManager.Size.Large => 100,
                _ => 0
            };
        }
    }
}
