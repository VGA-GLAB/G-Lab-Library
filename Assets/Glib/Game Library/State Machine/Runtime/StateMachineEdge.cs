// 日本語対応
using UnityEngine;

/// <summary>
/// ステートマシンのエッジデータを表現するクラス
/// </summary>
public class StateMachineEdge
{
    private StateMachineSO _stateMachineSO;
    private StateMachineNode _in;
    private StateMachineNode _out;
}
public interface IStateCondition
{
    public bool IsSuccess();
}
public interface IStateValue
{
    public string Name { get; }
}
[System.Serializable]
public class IntStateCondition : IStateCondition
{
    [SerializeField]
    int _baseline;
    [SerializeField]
    CompareMode _compareMode;

    public IntStateValue _target;

    public bool IsSuccess()
    {
        switch (_compareMode)
        {
            case CompareMode.GreaterThan: return _target.CurrentValue > _baseline;
            case CompareMode.LessThan: return _target.CurrentValue < _baseline;
            case CompareMode.Equal: return _target.CurrentValue == _baseline;
            case CompareMode.NotEqual: return _target.CurrentValue != _baseline;
        }
        throw new System.Exception();
    }
}
[System.Serializable]
public class FloatStateCondition : IStateCondition
{
    [SerializeField]
    float _baseline;
    [SerializeField]
    CompareMode _compareMode;

    public FloatStateValue _target;

    public bool IsSuccess()
    {
        switch (_compareMode)
        {
            case CompareMode.GreaterThan: return _target.CurrentValue > _baseline;
            case CompareMode.LessThan: return _target.CurrentValue < _baseline;
            case CompareMode.Equal: return Mathf.Abs(_target.CurrentValue - _baseline) < 0.0001f;
            case CompareMode.NotEqual: return Mathf.Abs(_target.CurrentValue - _baseline) > 0.0001f;
        }
        throw new System.Exception();
    }
}
[System.Serializable]
public class BoolStateCondition : IStateCondition
{
    public BoolStateCondition Clone()
    {
        var clone = new BoolStateCondition();
        clone.targetName = this.targetName;
        return clone;
    }

    public void SetStateMachine(StateMachineSO stateMachineSO)
    {
        _stateMachineSO = stateMachineSO;
    }

    private StateMachineSO _stateMachineSO = null;
    [SerializeField, StateMachineConditionName]
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
public struct IntStateValue : IStateValue
{
    [SerializeField]
    private string _name;
    [SerializeField]
    private int _currentValue;

    public string Name => _name;
    public int CurrentValue => _currentValue;
}
[System.Serializable]
public struct FloatStateValue : IStateValue
{
    [SerializeField]
    private string _name;
    [SerializeField]
    private float _currentValue;

    public string Name => _name;
    public float CurrentValue => _currentValue;
}
[System.Serializable]
public class BoolStateValue : IStateValue
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
[System.Serializable]
public enum CompareMode
{
    /// <summary> より大きい </summary>
    GreaterThan,
    /// <summary> より小さい </summary>
    LessThan,
    /// <summary> 等しい </summary>
    Equal,
    /// <summary> 等しくない </summary>
    NotEqual
}