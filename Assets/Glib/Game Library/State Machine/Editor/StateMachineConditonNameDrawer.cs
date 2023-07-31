// 日本語対応
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StateMachineConditionNameAttribute))]
public class StateMachineConditonNameDrawer : PropertyDrawer
{
    StateMachineNode currentTargetNode;
    string currentKey;
    GUIContent[] _contents;
    Dictionary<string, int> _indexData = new Dictionary<string, int>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        currentTargetNode = property.serializedObject.targetObject as StateMachineNode;
        currentKey = property.propertyPath;

        if (_contents == null || _contents.Length != currentTargetNode.StateMachine.Values.Length)
        {
            GUIContentsSetup();
        }

        if (!_indexData.ContainsKey(currentKey))
        {
            IndexSetup(property);
        }

        var oldIndex = _indexData[currentKey];
        _indexData[currentKey] = EditorGUI.Popup(position, label, _indexData[currentKey], _contents);

        if (oldIndex != _indexData[currentKey] || string.IsNullOrEmpty(property.stringValue))
        {
            if (_indexData[currentKey] >= 0 && _indexData[currentKey] < _contents.Length)
                property.stringValue = _contents[_indexData[currentKey]].text;
        }
    }
    private void GUIContentsSetup()
    {
        _contents = new GUIContent[currentTargetNode.StateMachine.Values.Length];

        for (int i = 0; i < _contents.Length; i++)
        {
            _contents[i] = new GUIContent(currentTargetNode.StateMachine.Values[i].Name);
        }

    }
    private void IndexSetup(SerializedProperty property)
    {
        if (!_indexData.ContainsKey(currentKey)) // 既にエントリがあるか確認
        {
            if (!string.IsNullOrEmpty(property.stringValue))
            {
                bool inputNameFound = false;
                for (int i = 0; i < _contents.Length; i++)
                {
                    if (_contents[i].text == property.stringValue)
                    {
                        inputNameFound = true;
                        _indexData[currentKey] = i; // 既存のエントリを更新

                        break;
                    }
                }
                if (!inputNameFound)
                {
                    _indexData[currentKey] = 0; // 既存のエントリを更新
                }
            }
            else
            {
                _indexData[currentKey] = 0; // 既存のエントリを更新
            }
        }

        // property.stringValueの値を適切な値に設定
        if (_indexData[currentKey] >= 0 && _indexData[currentKey] < _contents.Length)
            property.stringValue = _contents[_indexData[currentKey]].text;
    }

}
#endif