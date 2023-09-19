// 日本語対応
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Glib
{
    namespace InspectorExtension
    {
        [CustomPropertyDrawer(typeof(DisableEditInPlayModeAttribute))]
        public class DisableEditInPlayModeDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                // ゲームが再生中でない場合のみ、通常のフィールドを表示
                if (!EditorApplication.isPlaying)
                {
                    EditorGUI.PropertyField(position, property, label);
                }
                else
                {
                    // ゲームが再生中の場合、フィールドを無効化（編集不可）にする
                    GUI.enabled = false;
                    EditorGUI.PropertyField(position, property, label);
                    GUI.enabled = true;
                }
            }
        }
    }
}
#endif