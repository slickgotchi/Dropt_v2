namespace CarlosLab.Common
{
    public interface IImapWorld
    {
        IImapManager MapManager { get; }
        IImapSpaceHandler MapSpaceHandler { get; }
    }
}