namespace CarlosLab.UtilityIntelligence
{
    public interface ITask
    {
        bool IsRunning { get; }
        bool IsEnd { get; }
        internal RunStatus Run(float deltaTime);
        internal void Abort();
        internal void End();
    }
}