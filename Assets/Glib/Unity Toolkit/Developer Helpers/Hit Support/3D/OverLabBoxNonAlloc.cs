// 日本語対応
using System;
using System.Linq;
using UnityEngine;

namespace Glib
{
    namespace HitSupport
    {
        [Serializable]
        public class OverLabBoxNonAlloc
        {
            [SerializeField]
            [Range(1, 100)]
            private int _arrayCapacity = 1;

            [SerializeField]
            private Vector3 _dir = default;
            [SerializeField]
            private Vector3 _halfExtents = default;
            [SerializeField]
            private float _maxDistance = default;
            [SerializeField]
            private LayerMask _layerMask = default;

            private Collider[] results = null;

            public Collider[] GetOverlappingColliders(Transform origin, out int hitCount)
            {
                if (results == null || results.Length != _arrayCapacity)
                {
                    results = new Collider[_arrayCapacity];
                }

                hitCount = Physics.OverlapBoxNonAlloc(origin.rotation * (origin.position + _dir.normalized * _maxDistance), _halfExtents, results, origin.rotation, _layerMask);

                return results;
            }

#if UNITY_EDITOR
            [SerializeField]
            private bool _isDrawGizmos = false;
            [SerializeField]
            private Color _hitColor = Color.red;
            [SerializeField]
            private Color _noHitColor = Color.blue;
            public void OnDrawGizmos(Transform origin)
            {
                if (_isDrawGizmos == false)
                    return;

                var oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(origin.position, origin.rotation, Vector3.one);
                var hits = GetOverlappingColliders(origin, out int hitCount);
                if (hitCount > 0)
                {
                    Gizmos.color = _hitColor;
                }
                else
                {
                    Gizmos.color = _noHitColor;
                }
                Gizmos.DrawRay(Vector3.zero, _dir.normalized * _maxDistance);
                Gizmos.DrawCube(Vector3.zero + _dir.normalized * _maxDistance, _halfExtents * 2f);
                Gizmos.matrix = oldMatrix;
            }
#endif
        }
    }
}