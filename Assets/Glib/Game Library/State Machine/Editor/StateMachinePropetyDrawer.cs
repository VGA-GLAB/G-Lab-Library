// 日本語対応
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StateMachinePropetyAttribute))]
public class StateMachinePropetyDrawer : PropertyDrawer
{
    int _index = -1;
    GUIContent[] _contents;
    StateMachineRunner runner;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (_index == -1 || _contents.Length != runner.StateMachine.Values.Length)
        {
            Setup(position, property);
        }
        else
        {
            var oldIndex = _index;
            _index = EditorGUI.Popup(position, label, _index, _contents);

            if (_index != oldIndex)
            {
                property.stringValue = _contents[_index].text;
            }
        }
    }
    private void Setup(Rect position, SerializedProperty property)
    {
        // GUIContent[]のセットアップ
        var component = property.serializedObject.targetObject as Component;
        if (!component)
        {
            EditorGUI.LabelField(position, "Component型にキャストできませんでした。");
            return;
        }
        runner = component.GetComponent<StateMachineRunner>();
        if (!runner)
        {
            EditorGUI.LabelField(position, "StateMachineRunnerコンポーネントを取得できませんでした。");
            return;
        }
        if (runner.StateMachine == null)
        {
            EditorGUI.LabelField(position, "StateMachineを取得できませんでした。");
            return;
        }
        if (runner.StateMachine.Values == null || runner.StateMachine.Values.Length == 0)
        {
            EditorGUI.LabelField(position, "StateMachineのValuesを取得できませんでした。");
            return;
        }

        _contents = new GUIContent[runner.StateMachine.Values.Length];

        for (int i = 0; i < _contents.Length; i++)
        {
            _contents[i] = new GUIContent(runner.StateMachine.Values[i].Name);
        }

        // 復元処理
        if (!string.IsNullOrEmpty(property.stringValue))
        {
            bool inputNameFound = false;
            for (int i = 0; i < _contents.Length; i++)
            {
                if (_contents[i].text == property.stringValue)
                {
                    _index = i;
                    inputNameFound = true;
                    break;
                }
            }
            if (!inputNameFound)
                _index = 0;
        }
        else _index = 0;

        // プロパティに値を設定する。
        property.stringValue = _contents[_index].text;
    }
}
#endif