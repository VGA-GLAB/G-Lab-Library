// 日本語対応
using UnityEngine;
using System;

namespace Glib
{
    namespace HitSupport
    {
        [Serializable]
        public class SphereCast
        {
            [SerializeField]
            private Vector3 _dir = default;
            [SerializeField]
            private float _radius = default;
            [SerializeField]
            private float _maxDistance = default;
            [SerializeField]
            private LayerMask _layerMask = default;

            public RaycastHit GetHitInfo(Transform origin)
            {
                RaycastHit hit;
                Physics.SphereCast(origin.position, _radius, origin.rotation * _dir, out hit, _maxDistance, _layerMask);

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
                if (origin == null)
                    return;

                var hit = GetHitInfo(origin);

                if (hit.collider != null)
                {
                    Gizmos.color = _hitColor;
                    Gizmos.DrawRay(origin.position, origin.rotation * _dir.normalized * hit.distance);
                    Gizmos.DrawSphere(origin.position + origin.rotation * _dir.normalized * hit.distance, _radius);
                }
                else
                {
                    Gizmos.color = _noHitColor;
                    Gizmos.DrawRay(origin.position, origin.rotation * _dir.normalized * _maxDistance);
                    Gizmos.DrawSphere(origin.position + origin.rotation * _dir.normalized * _maxDistance, _radius);
                }
            }
#endif
        }
    }
}