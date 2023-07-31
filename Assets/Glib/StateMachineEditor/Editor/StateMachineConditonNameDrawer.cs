// 日本語対応
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StateMachineConditionNameAttribute))]
public class StateMachineConditonNameDrawer : PropertyDrawer
{
    int _index = -1;
    GUIContent[] _contents;

    Dictionary<string, int> _indexData = new Dictionary<string, int>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var stateNode = property.serializedObject.targetObject as StateMachineNode;
        if (!_indexData.ContainsKey(property.propertyPath) || _index == -1 || _contents.Length != stateNode.StateMachine.Values.Length)
        {
            SetupArray(property);
        }
        // 配列でなければ通常処理
        if (!property.isArray)
        {
            var oldIndex = _index;
            _index = EditorGUI.Popup(position, label, _index, _contents);

            if (_index != oldIndex)
            {
                property.stringValue = _contents[_index].text;
            }
        }
        else // 配列の処理
        {
            if (!_indexData.ContainsKey(property.propertyPath))
            {
                _indexData.Add(property.propertyPath, 0);
                property.stringValue = _contents[_indexData[property.propertyPath]].text;
            }

            var oldIndex = _indexData[property.propertyPath];
            _indexData[property.propertyPath] = EditorGUI.Popup(position, label, _indexData[property.propertyPath], _contents);

            if (oldIndex != _indexData[property.propertyPath] || string.IsNullOrEmpty(property.stringValue))
            {
                property.stringValue = _contents[_indexData[property.propertyPath]].text;
            }
        }
    }
    private void SetupArray(SerializedProperty property)
    {
        if (property.isArray)
        {
            _indexData = new Dictionary<string, int>();
        }

        var stateNode = property.serializedObject.targetObject as StateMachineNode;
        _contents = new GUIContent[stateNode.StateMachine.Values.Length];

        for (int i = 0; i < _contents.Length; i++)
        {
            _contents[i] = new GUIContent(stateNode.StateMachine.Values[i].Name);
        }

        if (!string.IsNullOrEmpty(property.stringValue))
        {
            bool inputNameFound = false;
            for (int i = 0; i < _contents.Length; i++)
            {
                if (_contents[i].text == property.stringValue)
                {
                    _index = i;
                    inputNameFound = true;
                    if (property.isArray)
                    {
                        _indexData.Add(property.propertyPath, i);
                        property.stringValue = _contents[i].text;
                    }
                    break;
                }

            }
            if (!inputNameFound)
            {
                _index = 0;
                if (property.isArray)
                {
                    _indexData.Add(property.propertyPath, 0);
                }
            }
        }
        else
        {
            _index = 0;
            if (property.isArray)
            {
                _indexData.Add(property.propertyPath, 0);
            }
        }

        if (!property.isArray)
        {
            // プロパティに値を設定する。
            property.stringValue = _contents[_index].text;
        }
    }
}
#endif