#if UNITY_EDITOR
// 日本語対応
using UnityEditor;
using UnityEngine.UIElements;

public class InspectorView : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits> { }

    private Editor _editor = null;

    public InspectorView()
    {

    }

    internal void UpdateSelection(NodeView nodeView)
    {
        Clear();

        UnityEngine.Object.DestroyImmediate(_editor);
        _editor = Editor.CreateEditor(nodeView.Node);
        IMGUIContainer container = new IMGUIContainer(() => { _editor.OnInspectorGUI(); });
        Add(container);
    }
}
#endif