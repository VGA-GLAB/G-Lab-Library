#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class StateMachineEditor : EditorWindow
{
    private StateMachineView _stateMachineView;
    private InspectorView _inspectorView;
    private ConditionsView _conditionsView;

    [MenuItem("Window/StateMachineEditor")]
    public static void OpenWindow()
    {
        StateMachineEditor wnd = GetWindow<StateMachineEditor>();
        wnd.titleContent = new GUIContent("StateMachineEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        //var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/StateMachineEditor/Editor/StateMachineEditor.uxml");
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(FindUxml("StateMachineEditor"));
        visualTree.CloneTree(root);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        //var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/StateMachineEditor/Editor/StateMachineEditor.uss");
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(FindUss("StateMachineEditor"));
        root.styleSheets.Add(styleSheet);

        _stateMachineView = root.Q<StateMachineView>();
        _inspectorView = root.Q<InspectorView>();
        _conditionsView = root.Q<ConditionsView>();

        _stateMachineView.OnNodeSelected = OnNodeSelectionChanged;

        OnSelectionChange();
    }
    StateMachineSO stateMachine;
    private void OnSelectionChange()
    {
        stateMachine = Selection.activeObject as StateMachineSO;

        if (stateMachine == null)
        {
            if (Selection.activeGameObject != null)
            {
                StateMachineRunner runner = Selection.activeGameObject.GetComponent<StateMachineRunner>();
                if (runner != null)
                {
                    stateMachine = runner.StateMachine;
                }
            }
        }

        if (Application.isPlaying)
        {
            if (stateMachine != null)
            {
                _stateMachineView?.PopulateView(stateMachine);
                _conditionsView?.UpdateSelection(stateMachine);
            }
        }

        if (stateMachine != null && AssetDatabase.CanOpenAssetInEditor(stateMachine.GetInstanceID()))
        {
            _stateMachineView?.PopulateView(stateMachine);
            _conditionsView?.UpdateSelection(stateMachine);
        }
    }
    void OnNodeSelectionChanged(StateMachineNodeView node)
    {
        _inspectorView.UpdateSelection(node);
    }
    private void OnGUI()
    {
        var e = Event.current;
        if (GetEventAction(e) && e.type == EventType.KeyDown && e.keyCode == KeyCode.S)
        {
            Debug.Log("Saved StateMachineEditor");
            if (stateMachine != null)
            {
                EditorUtility.SetDirty(stateMachine);
                AssetDatabase.SaveAssets();
            }
            e.Use();
        }
    }

    private bool GetEventAction(Event e)
    {
#if UNITY_EDITOR_WIN
        return e.control;
#else
    return e.command;
#endif
    }

    // OnOpenAssetAttributeはアセットが開かれた際に呼び出されるコールバック関数。
    [OnOpenAsset(0)]
    public static bool OnOpenWindow(int instanceID, int line)
    {
        // 開いたアセットがStateMachineSO型でなければ開かない。（リターンする。）
        if (EditorUtility.InstanceIDToObject(instanceID) is not StateMachineSO) return false;

        // ウィンドウを開く
        var window = EditorWindow.GetWindow<StateMachineEditor>();
        return true;
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }
    private void OnPlayModeChanged(PlayModeStateChange obj)
    {
        switch (obj)
        {
            case PlayModeStateChange.EnteredEditMode:
                OnSelectionChange();
                break;
            case PlayModeStateChange.ExitingEditMode:
                break;
            case PlayModeStateChange.EnteredPlayMode:
                OnSelectionChange();
                break;
            case PlayModeStateChange.ExitingPlayMode:
                break;
        }
    }
    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
    }

    bool isPreDrawCondition = false;
    Edge preEdge = null;

    private void OnInspectorUpdate()
    {
        _stateMachineView?.UpdateNodeState();

        var selects = _stateMachineView.selection;

        for (int i = 0; i < selects.Count; i++)
        {
            var edge = selects[i] as Edge;
            if (edge != null)
            {
                if (!isPreDrawCondition || preEdge != edge)
                {
                    _inspectorView.UpdateSelection(edge);
                }
                preEdge = edge;
                isPreDrawCondition = true;
                return;
            }
        }
        isPreDrawCondition = false;
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
        var assetHashs = AssetDatabase.FindAssets(name);
        var results = assetHashs
            .Select(hash => AssetDatabase.GUIDToAssetPath(Path.GetFileNameWithoutExtension(hash)))
            .Where(assetPath => Path.GetFileNameWithoutExtension(assetPath) == name)
            .Where(assetPath => Path.GetExtension(assetPath) == "." + extension);

        // 同じ名前のUIElementsが複数存在する(名前空間が違う等)場合の警告。
        if (1 < results.Count())
        {
            Debug.LogWarning($"\"{name}\"で{results.Count()}件の結果が見つかりました。\n\n{results.Aggregate((a, b) => a + "\n" + b)}");
        }

        return results.FirstOrDefault();
    }
}
#endif