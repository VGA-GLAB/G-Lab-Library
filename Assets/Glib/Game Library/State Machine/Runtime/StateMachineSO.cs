// 日本語対応
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "StateMachineData", menuName = "ScriptableObjects/StateMachine", order = 1)]
public class StateMachineSO : ScriptableObject
{
    [HideInInspector]
    [SerializeField]
    private EntryNode _entryNode;
    public EntryNode EntryNode { get => _entryNode; set => _entryNode = value; }

    private StateMachineNode _currentState = null;
    public StateMachineNode CurrentState => _currentState;

    [HideInInspector]
    [SerializeField]
    private List<StateMachineNode> _nodes = new List<StateMachineNode>();
    public List<StateMachineNode> Nodes => _nodes;

    [SerializeField]
    private BoolStateValue[] _values;
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
#if UNITY_EDITOR
        //Traverse(clone.EntryNode, n => clone.Nodes.Add(n));
#endif
        return clone;
    }
#if UNITY_EDITOR
    public StateMachineNode CreateNode(Type type)
    {
        StateMachineNode node = ScriptableObject.CreateInstance(type) as StateMachineNode;
        node.name = type.Name;
        node.GUID = GUID.Generate().ToString();
        node.SetStateMachine(this);

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

        //AssetDatabase.RemoveObjectFromAsset(node);
        Undo.DestroyObjectImmediate(node);

        AssetDatabase.SaveAssets();
    }

    public bool AddTo(StateMachineNode from, StateMachineNode to)
    {
        for (int i = 0; i < from.NextNodes.Count; i++)
        {
            if (from.NextNodes[i]._nextState == to)
            {
                return false;
            }
        }

        Undo.RecordObject(from, "State Machine (Add Next State)");

        if (from is EntryNode && from.NextNodes.Count == 1)
        {
            from.NextNodes.Clear();
        }

        var transition = new Transition(to, default);
        from.NextNodes.Add(transition);
        EditorUtility.SetDirty(from);

        return true;
    }
    public void RemoveTo(StateMachineNode from, StateMachineNode to)
    {
        Undo.RecordObject(from, "State Machine (Remove Next State)");

        Transition t = null;
        for (int i = 0; i < from.NextNodes.Count; i++)
        {
            if (from.NextNodes[i]._nextState == to)
            {
                t = from.NextNodes[i];
                break;
            }
        }

        if (t != null) from.NextNodes.Remove(t);
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

    public void Traverse(StateMachineNode node, Action<StateMachineNode> visiter)
    {
        if (node)
        {
            if (!node.IsInspected)
            {
                node.IsInspected = true;
                visiter.Invoke(node);
                var children = GetNextStates(node);
                children.ForEach(n => Traverse(n, visiter));
            }
            node.IsInspected = false;
        }
    }
#endif
}
