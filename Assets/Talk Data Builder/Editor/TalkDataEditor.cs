#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Glib.Talk;
using System.Linq;
using System.IO;

public class TalkDataEditor : EditorWindow
{
    private TalkDataEditorView _treeView;
    private InspectorView _inspectorView;

    [MenuItem("Window/Glib/TalkDataEditor")]
    public static void OpenWindow()
    {
        TalkDataEditor wnd = GetWindow<TalkDataEditor>();
        wnd.titleContent = new GUIContent("TalkDataEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var uxmlPath = FindUxml("TalkDataEditor");
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
        // var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/TalkDataEditor.uxml");
        visualTree.CloneTree(root);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        var ussPath = FindUss("TalkDataEditor");
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
        // var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/TalkDataEditor.uss");
        root.styleSheets.Add(styleSheet);

        _treeView = root.Q<TalkDataEditorView>();
        _inspectorView = root.Q<InspectorView>();

        _treeView.OnNodeSelected = OnNodeSelectionChangeed;

        OnSelectionChange();
    }

    private void OnSelectionChange()
    {
        TalkDataBuilder tree = Selection.activeObject as TalkDataBuilder;
        if (tree && AssetDatabase.CanOpenAssetInEditor(tree.GetInstanceID()))
        {
            _treeView.PopulateView(tree);
        }
    }
    private void OnNodeSelectionChangeed(NodeView node)
    {
        _inspectorView.UpdateSelection(node);
    }


    public static string FindUxml(string name)
    {
        return Find(name, "uxml");
    }

    public static string FindUss(string name)
    {
        return Find(name, "uss");
    }

    private static string Find(string name, string extension)
    {
        var assetPaths = AssetDatabase.FindAssets(name)
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Where(path => Path.GetExtension(path) == "." + extension)
            .ToArray();

        if (assetPaths.Length == 0)
        {
            Debug.LogWarning($"\"{name}\"の{extension}ファイルが見つかりませんでした。");
            return null;
        }
        else if (assetPaths.Length > 1)
        {
            Debug.LogWarning($"\"{name}\"で{assetPaths.Length}件の結果が見つかりました。\n\n{string.Join("\n", assetPaths)}");
        }

        return assetPaths[0];
    }
}
#endif