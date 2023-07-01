// 日本語対応
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumTypeAttribute))]
public class EnumTypeDrawer : PropertyDrawer
{
    private int _currentIndex = -1;
    private GUIContent[] _enumNames = null;
    private Type[] _enumTypes;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (_currentIndex == -1) SetUp(property);

        int oldIndex = _currentIndex;

        _currentIndex = EditorGUI.Popup(position, label, _currentIndex, _enumNames);

        if (oldIndex != _currentIndex)
        {
            var instance = Activator.CreateInstance(_enumTypes[_currentIndex]);
            property.managedReferenceValue = instance;
        }
    }
    private void SetUp(SerializedProperty property)
    {
        _enumTypes = GetEnumTypes(property);

        _enumNames = new GUIContent[_enumTypes.Length];
        for (int i = 0; i < _enumNames.Length; i++)
        {
            _enumNames[i] = new GUIContent();
            _enumNames[i].text = _enumTypes[i].Name;
        }

        Restoration(property);
    }
    private Type[] GetEnumTypes(SerializedProperty property)
    {
        Type propertyType = GetFieldType(property);

        var result = AppDomain.CurrentDomain.GetAssemblies() // 実行環境のアセンブリを取得
            .Where(a => a.FullName.Contains("glib") || a.FullName.Contains("Assembly-CSharp"))
            .SelectMany(s => s.GetTypes()) // アセンブリに存在する型をすべて取得
            .Where(p => propertyType.IsAssignableFrom(p) && p.IsEnum)// propertyの型がpの型に割り当て可能であるか && pはEnumであるか
            .ToArray();

        return result;
    }/// <summary> シリアライズ対象の型をType型で返す。 </summary>
    private Type GetFieldType(SerializedProperty property)
    {
        const BindingFlags bindingAttr =
                  BindingFlags.NonPublic |
                  BindingFlags.Public |
                  BindingFlags.FlattenHierarchy |
                  BindingFlags.Instance;

        var propertyPaths = property.propertyPath.Split('.');
        var fieldType = property.serializedObject.targetObject.GetType(); // シリアライズ元のクラスをType型でとってくる。
        for (int i = 0; i < propertyPaths.Length; i++)
        {
            FieldInfo field = fieldType.GetField(propertyPaths[i], bindingAttr);
            fieldType = field.FieldType;
        }
        return fieldType;
    }
    /// <summary> 復元 </summary>
    private void Restoration(SerializedProperty property)
    {
        if (property.managedReferenceValue != null)
        {
            bool found = false;
            for (int i = 0; i < _enumTypes.Length; i++)
            {
                if (_enumTypes[i] == property.managedReferenceValue.GetType())
                {
                    _currentIndex = i;
                    found = true;
                    break;
                }
            }
            if (!found) _currentIndex = 0;
        }
        else _currentIndex = 0;
    }
}