// 日本語対応
using UnityEngine;

namespace Glib
{
    namespace Talk
    {
        public class RootNode : Node
        {

        }
#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(RootNode))]
        public class RootNodeDrawer : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                // RootNodeはインスペクタから操作しないので、
                // デフォルトのインスペクタ描画を無効にする。
                // base.OnInspectorGUI();
            }
        }
#endif
    }
}