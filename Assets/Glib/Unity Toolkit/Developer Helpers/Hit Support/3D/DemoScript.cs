// 日本語対応
using UnityEngine;
using Glib.HitSupport;

public class DemoScript : MonoBehaviour
{
    [SerializeField]
    private BoxCast _boxCast;
    [SerializeField]
    private SphereCast _sphereCast;
    [SerializeField]
    private Raycast _raycast;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        _boxCast.OnDrawGizmos(this.transform);
        _sphereCast.OnDrawGizmos(this.transform);
        _raycast.OnDrawGizmos(this.transform);
    }
#endif
}
