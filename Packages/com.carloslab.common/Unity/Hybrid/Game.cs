#if UNITY_5_3_OR_NEWER

#region

using UnityEngine;

#endregion

namespace CarlosLab.Common
{
    public partial class Game
    {
        public bool IsPlaying => Application.isPlaying;
    }
}

#endif