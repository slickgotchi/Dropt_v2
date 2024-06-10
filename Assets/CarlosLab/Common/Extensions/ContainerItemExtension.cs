namespace CarlosLab.Common.Extensions
{
    internal static class ContainerItemExtension
    {
        public static void OnItemAdded(this IContainerItem item, string name)
        {
            item.OnItemAdded(name);
        }

        public static void OnItemRemoved(this IContainerItem item)
        {
            item.OnItemRemoved();
        }
    }
}