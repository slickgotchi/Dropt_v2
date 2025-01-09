namespace PickupItems.Orb
{
    public class CGHSTOrb : BaseOrb
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
            return size == PickupItemManager.Size.Large ? 5 : 1;
        }
    }
}