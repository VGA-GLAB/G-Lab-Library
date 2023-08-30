#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

// StateMachineのエディタ画面を管理するクラス。
public class StateMachineEditor : EditorWindow
{
    private StateMachineView _stateMachineView; // 背景がグリッド状のビュー
    private InspectorView _inspectorView;       // 左上のビュー。StateMachieneの状態を描画する。
    private ConditionsView _conditionsView;     // 左下のビュー。遷移条件を描画する。

    [MenuItem("Window/StateMachineEditor")]
    public static void OpenWindow()
    {
        StateMachineEditor wnd = GetWindow<StateMachineEditor>();
        wnd.titleContent = new GUIContent("StateMachineEditor");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        // Uxmlファイルの読み込み
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(FindUxml("StateMachineEditor"));
        visualTree.CloneTree(root);

        // Ussファイルの読み込み
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(FindUss("StateMachineEditor"));
        root.styleSheets.Add(styleSheet);

        // 各要素の取得。obj.Q<T>()は、objの子要素のT型のオブジェクトを取得する関数。
        _stateMachineView = root.Q<StateMachineView>();
        _inspectorView = root.Q<InspectorView>();
        _conditionsView = root.Q<ConditionsView>();

        _stateMachineView.OnNodeSelected = OnSelectionChanged;

        OnSelectionChange();
    }
    StateMachineSO stateMachine;
    private void OnSelectionChange()
    {
        // プロジェクトビューから選択されたオブジェクトを取得する。（StateMachineの取得に失敗した場合nullが帰ってくる。）
        stateMachine = Selection.activeObject as StateMachineSO;

        // ヒエラルキーウィンドウから選択されたオブジェクトが
        // StateMachineRunnerを持っており、RunnerにStateMachineが割り当てられていた場合に、
        // StateMachineを取得する。
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

        if (stateMachine != null &&
            (AssetDatabase.CanOpenAssetInEditor(stateMachine.GetInstanceID()) || Application.isPlaying))
        {
            _stateMachineView?.PopulateView(stateMachine);
            _conditionsView?.UpdateSelection(stateMachine);
        }
    }
    void OnSelectionChanged(StateMachineNodeView node)
    {
        _inspectorView.OnChangedSelection(node);
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