﻿#region

using System;
using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public abstract class Task : UtilityIntelligenceMemberTask
    {
        public Task Parent { get; internal set; }
        
        #region Awake
        
        protected bool Awakened { get; private set; }

        internal virtual void Awake()
        {
            if (Awakened)
                return;
            
            OnAwake();
            
            Awakened = true;
        }

        protected virtual void OnAwake()
        {
        }

        #endregion

        #region DecisionContext

        private DecisionContext context;

        public DecisionContext Context
        {
            get => context;
            internal set
            {
                context = value;
                OnContextChanged(value);
            }
        }
        protected virtual void OnContextChanged(DecisionContext newContext)
        {

        }

        #endregion
    }
}