namespace CarlosLab.Common
{
    public interface IGame
    {
        bool IsPlaying { get; }
    }

    public partial class Game : Singleton<Game>, IGame
    {
        private Game()
        {
        }
    }
}