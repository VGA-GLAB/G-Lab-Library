// 日本語対応
using UnityEngine;
using UnityEngine.UI;

public class StateMachineTestScript : MonoBehaviour
{
    [SerializeField]
    private Text currentState;
    [SerializeField]
    private Text value1Drawer;
    [SerializeField]
    private Text value2Drawer;
    [SerializeField]
    private Text value3Drawer;

    [SerializeField, StateMachinePropety]
    private string value1;
    [SerializeField, StateMachinePropety]
    private string value2;
    [SerializeField, StateMachinePropety]
    private string value3;

    private StateMachineRunner runner;

    private void Start()
    {
        runner = GetComponent<StateMachineRunner>();
    }

    private void Update()
    {
        currentState.text = runner.StateMachine.CurrentState.name;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            runner.StateMachine.SetValue(value1, !runner.StateMachine.GetValue(value1));
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            runner.StateMachine.SetValue(value2, !runner.StateMachine.GetValue(value2));
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            runner.StateMachine.SetValue(value3, !runner.StateMachine.GetValue(value3));
        }
        value1Drawer.text = $"Value1 = {runner.StateMachine.GetValue(value1)}";
        value2Drawer.text = $"Value2 = {runner.StateMachine.GetValue(value2)}";
        value3Drawer.text = $"Value3 = {runner.StateMachine.GetValue(value3)}";
    }
}
