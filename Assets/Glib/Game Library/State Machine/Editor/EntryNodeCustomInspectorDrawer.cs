// 日本語対応
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace StateMachine
{
    /// <summary>
    /// Entryノードのインスペクタを隠すクラス
    /// </summary>
    [CustomEditor(typeof(EntryNode))]
    public class EntryNodeCustomInspectorDrawer : Editor
    {
        // Entryノードはインスペクタに何も表示しない。
        public override void OnInspectorGUI()
        {
        }
    }
}
#endif