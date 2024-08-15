using UnityEngine;

namespace CarlosLab.UtilityIntelligence
{
    [Category("3D")]
    public class FaceTargetForever : ActionTask
    {
        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            var direction = TargetTransform.position - Transform.position;
            direction.Normalize();
            direction.y = 0;
            
            if(direction != Vector3.zero)
                Transform.forward = direction;
            
            return UpdateStatus.Running;
        }
    }
}