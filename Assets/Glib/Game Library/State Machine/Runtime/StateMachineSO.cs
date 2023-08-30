// 日本語対応
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(fileName = "StateMachineData", menuName = "ScriptableObjects/StateMachine", order = 1)]
public class StateMachineSO : ScriptableObject
{
    [HideInInspector]
    [SerializeField]
    private EntryNode _entryNode; // 開始ノード
    public EntryNode EntryNode { get => _entryNode; set => _entryNode = value; }

    private StateMachineNode _currentState = null; // 現在実行中のノード
    public StateMachineNode CurrentState => _currentState;

    [HideInInspector]
    [SerializeField]
    private List<StateMachineNode> _nodes = new List<StateMachineNode>(); // このステートマシンが所有する全てのノードリスト。
    public List<StateMachineNode> Nodes => _nodes;

    [SerializeField]
    private BoolStateValue[] _values; // このステートマシンが持つ条件用の値。
    public BoolStateValue[] Values => _values;

    private StateMachineRunner _runner = null;
    public StateMachineRunner Runner => _runner;

    public void Start()
    {
        foreach (var e in _nodes) e.Initialize(this);

        _currentState = _entryNode.NextNodes[0]._nextState;
        _currentState.Enter();
    }
    public void Update()
    {
        _currentState?.Update();
    }
    public void TransitionTo(StateMachineNode node)
    {
        _currentState?.Exit();
        _currentState = node;
        _currentState?.Enter();
    }
    public void SetValue(string name, bool value)
    {
        for (int i = 0; i < _values.Length; i++)
        {
            if (_values[i].Name == name)
            {
                _values[i].CurrentValue = value;
                return;
            }
        }
    }
    public bool GetValue(string name)
    {
        for (int i = 0; i < _values.Length; i++)
        {
            if (_values[i].Name == name)
            {
                return _values[i].CurrentValue;

            }
        }
        return false;
    }
    public virtual StateMachineSO Clone(StateMachineRunner runner)
    {
        StateMachineSO clone = Instantiate(this);
        clone._runner = runner;

        // オリジナルをキーに渡すことで、クローンを取得するディクショナリを作成する
        Dictionary<StateMachineNode, StateMachineNode> originalToClone = new Dictionary<StateMachineNode, StateMachineNode>();
        List<StateMachineNode> cloneNodes = new List<StateMachineNode>();
        // クローンを作成し、ディクショナリに登録する
        foreach (var e in this._nodes)
        {
            var cloneNode = Instantiate(e);
            originalToClone.Add(e, cloneNode);
            cloneNodes.Add(cloneNode);
        }
        // 開始ノードの割り当て
        clone.EntryNode = originalToClone[this.EntryNode] as EntryNode;
        clone._nodes = cloneNodes;
        // クローンのTransitionのセットアップを行う
        foreach (var e in originalToClone)
        {
            e.Value.CloneSetup(clone, e.Key, originalToClone);
        }

        return clone;
    }
#if UNITY_EDITOR
    public StateMachineNode CreateNode(Type type)
    {
        StateMachineNode node = ScriptableObject.CreateInstance(type) as StateMachineNode;
        node.name = type.Name;
        node.GUID = GUID.Generate().ToString();
        node.SetStateMachine(this); // StateMachineを渡しておく。

        Undo.RecordObject(this, "State Machine (Create Node)");
        _nodes.Add(node);

        AssetDatabase.AddObjectToAsset(node, this);
        Undo.RegisterCreatedObjectUndo(node, "State Machine (Create Node)");

        AssetDatabase.SaveAssets();
        return node;
    }
    public void DeleteNode(StateMachineNode node)
    {
        Undo.RecordObject(this, "State Machine (Delete Node)");
        _nodes.Remove(node);

        Undo.DestroyObjectImmediate(node);

        AssetDatabase.SaveAssets();
    }
    public bool TryConnect(StateMachineNode from, StateMachineNode to) // ノードの接続を試みる。
    {
        // 既に遷移先に含まれている場合は接続せず、falseを返す。
        bool containsToState = from.NextNodes.Any(node => node._nextState == to);
        if (containsToState) { return false; }

        Undo.RecordObject(from, "State Machine (Add Next State)");

        // fromがEntryノードの場合、遷移リストをクリアする。（Entryノードの遷移先は一つだけ。）
        if (from is EntryNode && from.NextNodes.Count == 1)
        {
            from.NextNodes.Clear();
        }

        // 遷移先を設定する。
        var transition = new Transition(to, null);
        from.NextNodes.Add(transition);
        EditorUtility.SetDirty(from);

        return true;
    }
    public void Disconnect(StateMachineNode from, StateMachineNode to) // エッジを切断する。
    {
        Undo.RecordObject(from, "State Machine (Remove Next State)");

        // 
        Transition transition = from.NextNodes.Find(t => t._nextState == to);

        if (transition != null) from.NextNodes.Remove(transition);
        EditorUtility.SetDirty(from);
    }
    public List<StateMachineNode> GetNextStates(StateMachineNode from)
    {
        var nexts = new List<StateMachineNode>();

        foreach (var e in from.NextNodes)
        {
            if (e._nextState != null)
            {
                nexts.Add(e._nextState);
            }
        }

        return nexts;
    }
#endif
}
