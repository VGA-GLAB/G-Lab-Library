// 日本語対応
using UnityEngine;

[System.Serializable]
public class BoolStateCondition
{
    public BoolStateCondition Clone(StateMachineSO stateMachineSO)
    {
        var clone = new BoolStateCondition();
        clone._stateMachineSO = stateMachineSO;
        clone.targetName = this.targetName;
        return clone;
    }

    public void SetStateMachine(StateMachineSO stateMachineSO)
    {
        _stateMachineSO = stateMachineSO;
    }

    private StateMachineSO _stateMachineSO = null;
    [SerializeField, StateMachinePropertyArray]
    private string targetName;

    public string TargetName => targetName;
    public bool IsSuccess()
    {
        foreach (var e in _stateMachineSO.Values)
        {
            if (e.Name == targetName && e.CurrentValue)
            {
                return true;
            }
        }
        return false;
    }
}
[System.Serializable]
public class BoolStateValue
{
    [SerializeField]
    private string _name;
    [SerializeField]
    private bool _currentValue;

    public string Name => _name;
    public bool CurrentValue
    {
        get => _currentValue; set => _currentValue = value;
    }

    public BoolStateValue Clone()
    {
        var clone = new BoolStateValue();
        clone._name = this._name;
        clone._currentValue = this._currentValue;
        return clone;
    }
}