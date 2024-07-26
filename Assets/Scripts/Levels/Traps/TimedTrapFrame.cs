using System;
using UnityEngine;

namespace Level.Traps
{
    [Serializable]
    public struct TimedTrapFrame
    {
        public Sprite Frame;
        public int TimeMs;
        public bool IsActive;
        public bool IsGotoToNextGroup;
    }
}