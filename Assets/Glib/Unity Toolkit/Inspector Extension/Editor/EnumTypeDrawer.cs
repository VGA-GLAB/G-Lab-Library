#if UNITY_EDITOR
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
    private object _instance = null;
    private string[] _typeFullNameArray;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ManagedReference) return;
        if (_currentIndex == -1) SetUp(property);

        int oldIndex = _currentIndex;

        // これがないと選択がバグる...なぜなのか...
        _currentIndex = Array.IndexOf(_typeFullNameArray, property.managedReferenceFullTypename);

        _currentIndex = EditorGUI.Popup(position, label, _currentIndex, _enumNames);

        if (_currentIndex == -1) _currentIndex = oldIndex;
        if (_currentIndex != oldIndex)
        {
            _instance = Activator.CreateInstance(_enumTypes[_currentIndex]);
            property.managedReferenceValue = _instance;
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

        _instance = Activator.CreateInstance(_enumTypes[_currentIndex]);
        property.managedReferenceValue = _instance;
    }
    private Type[] GetEnumTypes(SerializedProperty property)
    {
        var result = AppDomain.CurrentDomain.GetAssemblies() // 実行環境のアセンブリを取得
            .Where(a => a.FullName.Contains("glib") || a.FullName.Contains("Assembly-CSharp"))
            .SelectMany(s => s.GetTypes()) // アセンブリに存在する型をすべて取得
            .Where(p => typeof(Enum).IsAssignableFrom(p) && p.IsEnum)// propertyの型がpの型に割り当て可能であるか && pはEnumであるか
            .ToArray();

        _typeFullNameArray =
            result.Select(type => string.Format("{0} {1}", type.Assembly.ToString().Split(',')[0], type.FullName)).ToArray();

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
#endif