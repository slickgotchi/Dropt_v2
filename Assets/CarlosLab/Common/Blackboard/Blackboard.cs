#region

using CarlosLab.Common;

#endregion

namespace CarlosLab.Common
{
    public class Blackboard : ItemValueContainer<Variable>
    {
        public TVariable GetVariable<TVariable>(string name)
            where TVariable : Variable
        {
            return GetItem<TVariable>(name);
        }
    }
}