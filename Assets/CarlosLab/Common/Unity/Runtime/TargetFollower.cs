#region

using UnityEngine;

#endregion

namespace CarlosLab.Common
{
    public class TargetFollower : MonoBehaviour
    {
        private Transform target;

        private void LateUpdate()
        {
            if (target != null)
                transform.position = target.transform.position;
        }

        public void Init(Transform target)
        {
            this.target = target;
        }
    }
}