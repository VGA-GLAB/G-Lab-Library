// 日本語対応
using UnityEngine;

namespace Glib
{
    namespace HitSupport
    {
        [System.Serializable]
        public class Raycast
        {
            [SerializeField]
            private Vector3 _dir = default;
            [SerializeField]
            private float _maxDistance = default;
            [SerializeField]
            private LayerMask _layerMask = default;
            [SerializeField]
            private Vector3 _offset = Vector3.zero;

            public RaycastHit GetHitInfo(Transform origin)
            {
                RaycastHit hit;
                Physics.Raycast(origin.position + _offset, origin.rotation * _dir, out hit, _maxDistance, _layerMask);

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
                    // Rayがヒットしたかどうかで色を変える。
                    RaycastHit hit;
                    if (Physics.Raycast(origin.position + _offset, origin.rotation * _dir, out hit, _maxDistance, _layerMask))
                    {
                        Debug.DrawRay(
                            origin.position + _offset, // 開始位置
                            hit.point - (origin.position + _offset), //Rayの方向と距離
                            _hitColor, // ヒットした場合の色
                            0, // ラインを表示する時間（秒単位）
                            false); // ラインがカメラから近いオブジェクトによって隠された場合にラインを隠すかどうか
                    }
                    else
                    {
                        Debug.DrawRay(
                            origin.position + _offset,
                           (origin.rotation * _dir).normalized * _maxDistance,
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