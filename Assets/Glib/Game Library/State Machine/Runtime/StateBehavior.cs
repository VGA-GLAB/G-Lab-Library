// 日本語対応
using UnityEngine;

namespace StateMachine
{
    public abstract class StateBehavior : IState
    {
        protected StateMachineSO _stateMachine;

        public void Init(StateMachineSO stateMachine)
        {
            _stateMachine = stateMachine;
            Start();
        }

        public virtual void Start()
        {

        }
        public virtual void Enter()
        {

        }

        public virtual void Exit()
        {

        }

        public virtual void Update()
        {

        }
    }
}