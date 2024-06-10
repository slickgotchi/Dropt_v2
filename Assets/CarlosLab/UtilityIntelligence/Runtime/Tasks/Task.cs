#region

using System;
using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public abstract class Task : DecisionTask, IModel
    {
        #region Properties

        public Task Parent { get; internal set; }

        protected bool IsAwakened { get; private set; }
        
        #endregion

        #region Functions

        internal virtual void Awake()
        {
            if (IsAwakened)
                return;
            IsAwakened = true;
            OnAwake();
        }

        protected virtual void OnAwake()
        {
        }

        #endregion
    }
}