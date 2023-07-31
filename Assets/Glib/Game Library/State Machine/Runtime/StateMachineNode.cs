// 日本語対応
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ステートマシンのノードデータを表現するクラス
/// </summary>
public class StateMachineNode : ScriptableObject
{
    [SerializeField]
    private string _name = "State Machine Node";
    public string Name { get => _name; set => _name = value; }

    [SerializeField]
    [HideInInspector]
    private string guid;
    public string GUID { get => guid; set => guid = value; }

    [SerializeField]
    private List<Transition> _nextNodes = new List<Transition>();
    public List<Transition> NextNodes => _nextNodes;

    [SerializeField]
    [HideInInspector]
    private Vector2 _position;
    public Vector2 Position { get => _position; set => _position = value; }

    [Header("実行するクラス")]
    [SerializeField, SerializeReference, SubclassSelector]
    private IState[] _states;

    public bool IsRunning { get; private set; }
    public bool IsInspected { get; set; } = false;

    [SerializeField]
    [HideInInspector]
    private StateMachineSO _stateMachine;
    public StateMachineSO StateMachine => _stateMachine;

    public System.Action OnValueChanged;

    private void OnValidate()
    {
        this.name = this._name;
        OnValueChanged?.Invoke();
    }
    public void Initialize(StateMachineSO stateMachineSO)
    {
        foreach (var e in _states)
        {
            e.Init(stateMachineSO);
        }
        foreach (var e in _nextNodes)
        {
            foreach (var i in e._conditions)
            {
                i.SetStateMachine(stateMachineSO);
            }
        }
    }
    public void SetStateMachine(StateMachineSO stateMachine)
    {
        _stateMachine = stateMachine;
    }
    public virtual void Enter()
    {
        IsRunning = true;
        foreach (var e in _states) e?.Enter();
    }
    public virtual void Update()
    {
        // 毎フレーム実行する処理
        foreach (var e in _states) e?.Update();

        // ステートの遷移
        foreach (var e in _nextNodes) // 次のノードを一個ずつ取り出してチェックする
        {
            // 条件を表現する値が存在しない場合、遷移条件を確認する必要がないので次の遷移をチェックする。
            if (e._conditions == null || e._conditions.Length == 0) continue;
            // 設定された条件の値が全て満たされているかどうかチェックする。

            int isSuccessCounter = 0;
            foreach (var k in _stateMachine.Values) // ステートマシンの値を一個ずつ取り出す
            {
                foreach (var i in e._conditions) // 遷移条件を一個ずつ取り出して
                {
                    if (i.TargetName == k.Name && k.CurrentValue)
                    {
                        isSuccessCounter++;
                    }
                }
            }
            if (isSuccessCounter == e._conditions.Length)
            {
                _stateMachine.TransitionTo(e._nextState);
                return;
            }
        }
    }
    public virtual void Exit()
    {
        foreach (var e in _states) e?.Exit();
        IsRunning = false;
    }

    public virtual StateMachineNode CloneSetup(StateMachineSO stateMachineClone, StateMachineNode original, Dictionary<StateMachineNode, StateMachineNode> nodes)
    {
        _stateMachine = stateMachineClone;
        // List<Transition> _nextNodes のコピーを行う
        this._nextNodes = new List<Transition>();
        for (int i = 0; i < original._nextNodes.Count; i++)
        {
            BoolStateCondition[] conditions = new BoolStateCondition[original._nextNodes[i]._conditions.Length];

            for (int j = 0; j < conditions.Length; j++)
            {
                conditions[j] = original._nextNodes[i]._conditions[j].Clone();
                conditions[j].SetStateMachine(stateMachineClone);
            }

            var t = new Transition(nodes[original._nextNodes[i]._nextState], conditions);
            this._nextNodes.Add(t);
        }

        // 実行するクラス IState[] _states のコピーを行う
        this._states = new IState[original._states.Length];
        for (int i = 0; i < this._states.Length; i++)
        {
            if (original._states[i] == null) continue;
            this._states[i] = (IState)Activator.CreateInstance(original._states[i].GetType());
            //Debug.Log(original._states[i].GetHashCode());
            //Debug.Log(this._states[i].GetHashCode());
        }

        return this;
    }
}