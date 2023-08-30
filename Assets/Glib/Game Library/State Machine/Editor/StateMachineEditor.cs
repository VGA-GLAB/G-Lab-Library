#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

// StateMachine�̃G�f�B�^��ʂ��Ǘ�����N���X�B
public class StateMachineEditor : EditorWindow
{
    private StateMachineView _stateMachineView; // �w�i���O���b�h��̃r���[
    private InspectorView _inspectorView;       // ����̃r���[�BStateMachiene�̏�Ԃ�`�悷��B
    private ConditionsView _conditionsView;     // �����̃r���[�B�J�ڏ�����`�悷��B

    [MenuItem("Window/StateMachineEditor")]
    public static void OpenWindow()
    {
        StateMachineEditor wnd = GetWindow<StateMachineEditor>();
        wnd.titleContent = new GUIContent("StateMachineEditor");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        // Uxml�t�@�C���̓ǂݍ���
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(FindUxml("StateMachineEditor"));
        visualTree.CloneTree(root);

        // Uss�t�@�C���̓ǂݍ���
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(FindUss("StateMachineEditor"));
        root.styleSheets.Add(styleSheet);

        // �e�v�f�̎擾�Bobj.Q<T>()�́Aobj�̎q�v�f��T�^�̃I�u�W�F�N�g���擾����֐��B
        _stateMachineView = root.Q<StateMachineView>();
        _inspectorView = root.Q<InspectorView>();
        _conditionsView = root.Q<ConditionsView>();

        _stateMachineView.OnNodeSelected = OnSelectionChanged;

        OnSelectionChange();
    }
    StateMachineSO stateMachine;
    private void OnSelectionChange()
    {
        // �v���W�F�N�g�r���[����I�����ꂽ�I�u�W�F�N�g���擾����B�iStateMachine�̎擾�Ɏ��s�����ꍇnull���A���Ă���B�j
        stateMachine = Selection.activeObject as StateMachineSO;

        // �q�G�����L�[�E�B���h�E����I�����ꂽ�I�u�W�F�N�g��
        // StateMachineRunner�������Ă���ARunner��StateMachine�����蓖�Ă��Ă����ꍇ�ɁA
        // StateMachine���擾����B
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

    // OnOpenAssetAttribute�̓A�Z�b�g���J���ꂽ�ۂɌĂяo�����R�[���o�b�N�֐��B
    [OnOpenAsset(0)]
    public static bool OnOpenWindow(int instanceID, int line)
    {
        // �J�����A�Z�b�g��StateMachineSO�^�łȂ���ΊJ���Ȃ��B�i���^�[������B�j
        if (EditorUtility.InstanceIDToObject(instanceID) is not StateMachineSO) return false;

        // �E�B���h�E���J��
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

        // �������O��UIElements���������݂���(���O��Ԃ��Ⴄ��)�ꍇ�̌x���B
        if (1 < results.Count())
        {
            Debug.LogWarning($"\"{name}\"��{results.Count()}���̌��ʂ�������܂����B\n\n{results.Aggregate((a, b) => a + "\n" + b)}");
        }

        return results.FirstOrDefault();
    }
}
#endif