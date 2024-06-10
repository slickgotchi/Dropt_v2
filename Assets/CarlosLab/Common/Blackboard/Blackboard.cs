#region

using CarlosLab.Common;

#endregion

namespace CarlosLab.Common
{
    public class Blackboard : ItemValueContainer<Variable>
    {
        public Variable<TValue> GetVariable<TValue>(string name)
        {
            return GetItem<TValue>(name) as Variable<TValue>;
        }
    }
}