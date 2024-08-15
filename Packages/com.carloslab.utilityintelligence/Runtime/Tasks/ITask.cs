namespace CarlosLab.UtilityIntelligence
{
    public interface ITask
    {
        ExecuteStatus Execute(float deltaTime);
        internal void Abort();
        internal void Reset();
    }
}