namespace PickupItems.Orb
{
    public class HpOrb : BaseOrb
    {
        protected override int CalculateValue(PickupItemManager.Size size)
        {
            return size switch
            {
                PickupItemManager.Size.Tiny => 10,
                PickupItemManager.Size.Small => 20,
                PickupItemManager.Size.Medium => 30,
                PickupItemManager.Size.Large => 50,
                _ => 0
            };
        }
    }
}
