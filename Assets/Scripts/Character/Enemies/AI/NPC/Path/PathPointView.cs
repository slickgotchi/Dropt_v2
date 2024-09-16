using UnityEngine;

namespace AI.NPC.Path
{
    public sealed class PathPointView : MonoBehaviour
    {
        private Vector3? _position;
        private MeshRenderer _renderer;

        public Vector3 position => _position ?? transform.position;

        public Vector3 localPosition => _position ?? transform.localPosition;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();

            if (_renderer != null)
                _renderer.enabled = false;
        }
    }
}