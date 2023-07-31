// 日本語対応
using UnityEngine;

// ステートマシンを利用するためのコントローラーコンポーネント
public class StateMachineRunner : MonoBehaviour
{
    [SerializeField]
    private StateMachineSO _stateMachine;

    public StateMachineSO StateMachine => _stateMachine;

    private void Start()
    {
        _stateMachine = _stateMachine.Clone(this);
        _stateMachine.Start();
    }
    private void Update()
    {
        _stateMachine.Update();
    }
}
