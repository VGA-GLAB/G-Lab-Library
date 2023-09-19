// 日本語対応
using System;
using System.Collections.Generic;
using Glib.InspectorExtension;
using UnityEngine;

/// <summary>
/// ステートマシンのノードデータを表現するクラス
/// </summary>
public class StateMachineNode : ScriptableObject
{
    // 自分のオーナーとなるステートマシン。
    [SerializeField]
    [HideInInspector]
    private StateMachineSO _stateMachine;
    public StateMachineSO StateMachine => _stateMachine;

    // このステートの名前。
    [SerializeField]
    private string _name = "State Machine Node";
    public string Name { get => _name; set => _name = value; }

    // Viewの復元のための値。
    [SerializeField]
    [HideInInspector]
    private string guid;
    public string GUID { get => guid; set => guid = value; }

    // Viewの座標を表現する。
    [SerializeField]
    [HideInInspector]
    private Vector2 _position;
    public Vector2 Position { get => _position; set => _position = value; }

    // 遷移データのリスト。（遷移データには、遷移条件と遷移先の情報が含まれる。）
    [SerializeField]
    private List<Transition> _nextNodes = new List<Transition>();
    public List<Transition> NextNodes => _nextNodes;

    [Header("実行するクラス")]
    [SerializeField, SerializeReference, SubclassSelector]
    private IState[] _states;

    // 現在このステートが実行中かどうか表現する値。
    public bool IsRunning { get; private set; }

    public Action OnValueChanged; // Inspectorから値が変更された時に発火するイベント。

    private void OnValidate()
    {
        this.name = this._name;
        OnValueChanged?.Invoke();
    }
    public void Initialize(StateMachineSO stateMachineSO)
    {
        // 実行されるオブジェクトの初期化。（StateMachineを渡す。）
        foreach (var e in _states)
        {
            e.Init(stateMachineSO);
        }
        // 遷移条件オブジェクトの初期化。（条件オブジェクトにStateMachineを渡す。）
        foreach (var transition in _nextNodes)
        {
            foreach (var condition in transition._conditions)
            {
                condition.SetStateMachine(stateMachineSO);
            }
        }
    }
    public void SetStateMachine(StateMachineSO stateMachine)
    {
        _stateMachine = stateMachine;
    }
    public virtual void Enter()
    {
        IsRunning = true; // 現在実行中である事を表現する。
        foreach (var e in _states) e?.Enter();
    }
    public virtual void Update()
    {
        // 毎フレーム実行する処理
        foreach (var e in _states) e?.Update();

        // 遷移チェックと実行（毎フレームやる必要は特に無いので後で修正する。）
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
        IsRunning = false; // 現在実行中ではない事を表現する。
    }

    // 引数に渡された情報を基にCloneのセットアップを行う。また、自身はCloneであるとする。
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
                conditions[j] = original._nextNodes[i]._conditions[j].Clone(stateMachineClone);
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
        }

        return this;
    }
}