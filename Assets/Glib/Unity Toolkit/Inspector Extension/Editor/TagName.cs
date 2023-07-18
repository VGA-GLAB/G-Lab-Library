#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// インスペクタでTagを選択する用クラス
/// </summary>
[CustomPropertyDrawer(typeof(TagNameAttribute))]
public class TestTagDropBox : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
        EditorGUI.EndProperty();
    }
}
#endif