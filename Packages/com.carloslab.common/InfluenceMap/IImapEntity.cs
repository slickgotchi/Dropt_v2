#region

using CarlosLab.Common;

#endregion

namespace CarlosLab.Common
{
    public interface IImapEntity : IEntity

    {
        IImapWorld MapWorld { get; }
        IImapSpaceHandler MapSpaceHandler { get; }
        IImapManager MapManager { get; }

        float GetInfluence(int layerId, int mapType);
    }
}