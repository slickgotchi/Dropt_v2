namespace PickupItems.Orb
{
    public class HpOrb : BaseOrb
    {
        protected override int CalculateValue(PickupItemManager.Size size)
        {
            return size switch
            {
                PickupItemManager.Size.Tiny => 20,
                PickupItemManager.Size.Small => 20,
                PickupItemManager.Size.Medium => 100,
                PickupItemManager.Size.Large => 100,
                _ => 0
            };
        }
    }
}
