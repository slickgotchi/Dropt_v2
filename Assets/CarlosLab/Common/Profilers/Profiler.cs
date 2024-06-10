
namespace CarlosLab.Common
{
    public static class Profiler
    {
        public static ProfilerSampler Sample(string name)
        {
            return new ProfilerSampler(name);
        }
    }
}

