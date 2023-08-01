// 日本語対応
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StateMachineNode))]
public class StateMachineNodePropertyDrawer : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_name"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_states"));
        serializedObject.ApplyModifiedProperties();
    }
}
#endif