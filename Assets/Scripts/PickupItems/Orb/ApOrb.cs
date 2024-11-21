namespace PickupItems.Orb
{
    public class ApOrb : BaseOrb
    {
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
