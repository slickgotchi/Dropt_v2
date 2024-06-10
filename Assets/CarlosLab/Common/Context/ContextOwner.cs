﻿using System;

namespace CarlosLab.Common
{
    public class ContextOwner<TContext>
        where TContext : IContext
    {
        private TContext context;

        public TContext Context
        {
            get => context;
            internal set
            {
                context = value;
                OnContextChanged(context);
                ContextChanged?.Invoke(context);
            }
        }
        
        public event Action<TContext> ContextChanged;

        public virtual void OnContextChanged(TContext context)
        {
        }
    }
}