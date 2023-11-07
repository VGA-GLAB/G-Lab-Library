using UnityEngine;
using System;

namespace Glib
{
    namespace HitSupport
    {
        [Serializable]
        public class Raycast2D
        {
            [SerializeField]
            private Vector2 _dir = default;
            [SerializeField]
            private float _maxDistance = default;
            [SerializeField]
            private LayerMask _layerMask = default;
            [SerializeField]
            private Vector2 _offset = Vector2.zero;

            public Vector2 Dir { get => _dir; set => _dir = value; }
            public float MaxDistance { get => _maxDistance; set => _maxDistance = value; }
            public LayerMask LayerMask { get => _layerMask; set => _layerMask = value; }
            public Vector2 Offset { get => _offset; set => _offset = value; }

            public RaycastHit2D GetHitInfo(Transform origin)
            {
                RaycastHit2D hit = Physics2D.Raycast((Vector2)origin.position + _offset, _dir, _maxDistance, _layerMask);

                return hit;
            }
            public bool IsHit(Transform origin)
            {
                return GetHitInfo(origin).collider != null;
            }

#if UNITY_EDITOR
            [SerializeField]
            private bool _isDrawGizmo = true;
            [SerializeField]
            private Color _hitColor = Color.red;
            [SerializeField]
            private Color _noHitColor = Color.blue;

            public void OnDrawGizmos(Transform origin)
            {
                if (_isDrawGizmo)
                {
                    RaycastHit2D hit = GetHitInfo(origin);
                    Vector2 originPos = origin.position;
                    if (hit.collider != null)
                    {
                        Debug.DrawRay(
                            originPos + _offset,
                            hit.point - (originPos + _offset),
                            _hitColor,
                            0,
                            false);
                    }
                    else
                    {
                        Debug.DrawRay(
                            originPos + _offset,
                            _dir.normalized * _maxDistance,
                            _noHitColor,
                            0,
                            false);
                    }
                }
            }
#endif
        }
    }
}
