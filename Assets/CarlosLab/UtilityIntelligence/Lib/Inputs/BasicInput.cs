namespace CarlosLab.UtilityIntelligence
{
    public abstract class BasicInput<T> : Input<T>
    {
    }

    public class BasicInputInt : BasicInput<int>
    {
    }
    
    public class BasicInputLong : BasicInput<long>
    {
    }
    
    public class BasicInputDouble : BasicInput<double>
    {
    }

    public class BasicInputFloat : BasicInput<float>
    {
    }

    public class BasicInputBool : BasicInput<bool>
    {
    }
}