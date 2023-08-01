// 日本語対応
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ConditionsView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<ConditionsView, VisualElement.UxmlTraits> { }

    Editor _editor;

    internal void UpdateSelection(StateMachineSO stateMachine)
    {
        Clear();

        Object.DestroyImmediate(_editor);
        _editor = Editor.CreateEditor(stateMachine);

        IMGUIContainer container = new IMGUIContainer(() =>
        {
            if (_editor.target)
            {
                _editor.OnInspectorGUI();
            }
        });

        Add(container);
    }
}
#endif