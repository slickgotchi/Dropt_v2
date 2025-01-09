
namespace PickupItems.Orb
{
    public class ApOrb : BaseOrb
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
                PickupItemManager.Size.Tiny => 10,
                PickupItemManager.Size.Small => 10,
                PickupItemManager.Size.Medium => 50,
                PickupItemManager.Size.Large => 50,
                _ => 0
            };
        }
    }
}
