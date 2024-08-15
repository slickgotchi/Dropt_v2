using CarlosLab.Common;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence
{
    public abstract class SetParam : AnimatorActionTask
    {
        public VariableReference<string> ParamName;
    }
}