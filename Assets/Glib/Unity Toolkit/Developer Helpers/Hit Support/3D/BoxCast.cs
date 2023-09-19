// 日本語対応
using UnityEngine;
using System;

namespace Glib
{
    namespace HitSupport
    {
        [Serializable]
        public class BoxCast
        {
            [SerializeField]
            private Vector3 _dir = default;
            [SerializeField]
            private Vector3 _halfExtents = default;
            [SerializeField]
            private float _maxDistance = default;
            [SerializeField]
            private LayerMask _layerMask = default;

            public RaycastHit GetHitInfo(Transform origin)
            {
                RaycastHit hit;
                Physics.BoxCast(origin.position, _halfExtents, origin.rotation * _dir, out hit, origin.rotation, _maxDistance, _layerMask);

                return hit;
            }
            public bool IsHit(Transform origin)
            {
                return GetHitInfo(origin).collider != null;
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
                var hit = GetHitInfo(origin);
                if (hit.collider != null)
                {
                    Gizmos.color = _hitColor;
                    Gizmos.DrawRay(Vector3.zero, _dir.normalized * hit.distance);
                    Gizmos.DrawCube(Vector3.zero + _dir.normalized * hit.distance, _halfExtents * 2f);
                }
                else
                {
                    Gizmos.color = _noHitColor;
                    Gizmos.DrawRay(Vector3.zero, _dir.normalized * _maxDistance);
                    Gizmos.DrawCube(Vector3.zero + _dir.normalized * _maxDistance, _halfExtents * 2f);
                }
                Gizmos.matrix = oldMatrix;
            }
#endif
        }
    }
}