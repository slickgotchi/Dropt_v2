namespace CarlosLab.Common.Extensions
{
    internal static class WorldExtension
    {
        public static void Update(this IWorld world, float deltaTime)
        {
            world.Tick(deltaTime);
        }
    }
}