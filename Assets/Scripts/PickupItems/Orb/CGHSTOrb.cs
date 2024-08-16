namespace PickupItems.Orb
{
    public class CGHSTOrb : BaseOrb
    {
        protected override int CalculateValue(PickupItemManager.Size size)
        {
            return size == PickupItemManager.Size.Large ? 5 : 1;
        }
    }
}