
using UnityEngine;

namespace CarlosLab.UtilityIntelligence
{
    public class Log : ActionTask
    {
        public string Message;

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            Debug.Log($"LogTask Message: {Message}");
            return UpdateStatus.Success;
        }
    }
}