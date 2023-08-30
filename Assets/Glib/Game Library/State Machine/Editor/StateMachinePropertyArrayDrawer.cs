// 日本語対応
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// StateMachineのValueからキーとなる文字列を引っ張ってきて
// ドロップダウンリストで選択できるようにするための属性。（配列用）
[CustomPropertyDrawer(typeof(StateMachinePropertyArrayAttribute))]
public class StateMachinePropertyArrayDrawer : PropertyDrawer
{
    StateMachineNode _node; // 条件を設定する対象となるNode。
    string _key;
    GUIContent[] _contents; // 選択肢となる文字列の配列
    Dictionary<string, int> _indexData = new Dictionary<string, int>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        _node = property.serializedObject.targetObject as StateMachineNode;
        _key = property.propertyPath;

        if (_contents == null || _contents.Length != _node.StateMachine.Values.Length)
        {
            GUIContentsSetup();
        }

        if (!_indexData.ContainsKey(_key))
        {
            IndexSetup(property);
        }

        var oldIndex = _indexData[_key];
        var newIndex = EditorGUI.Popup(position, label, _indexData[_key], _contents);

        if (oldIndex != newIndex || string.IsNullOrEmpty(property.stringValue))
        {
            if (newIndex >= 0 && newIndex < _contents.Length)
            {
                _indexData[_key] = newIndex;
                property.stringValue = _contents[newIndex].text;
            }
        }
    }
    private void GUIContentsSetup()
    {
        _contents = new GUIContent[_node.StateMachine.Values.Length];

        for (int i = 0; i < _contents.Length; i++)
        {
            _contents[i] = new GUIContent(_node.StateMachine.Values[i].Name);
        }

    }
    private void IndexSetup(SerializedProperty property)
    {
        if (!string.IsNullOrEmpty(property.stringValue))
        {
            bool inputNameFound = false;
            for (int i = 0; i < _contents.Length; i++)
            {
                if (_contents[i].text == property.stringValue)
                {
                    inputNameFound = true;
                    _indexData[_key] = i;

                    break;
                }
            }
            if (!inputNameFound)
            {
                _indexData[_key] = 0;
            }
        }
        else
        {
            _indexData[_key] = 0;
        }

        // property.stringValueの値を適切な値に設定
        if (_indexData[_key] >= 0 && _indexData[_key] < _contents.Length)
            property.stringValue = _contents[_indexData[_key]].text;
    }
}
#endif