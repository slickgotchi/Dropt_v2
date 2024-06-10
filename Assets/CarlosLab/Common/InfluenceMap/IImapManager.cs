#region

using CarlosLab.Common;

#endregion

namespace CarlosLab.Common
{
    public interface IImapManager
    {
        void AddInfluence(int layerId, int mapType, Int2 cellIndexWorld, int radius, float magnitude);
    }
}