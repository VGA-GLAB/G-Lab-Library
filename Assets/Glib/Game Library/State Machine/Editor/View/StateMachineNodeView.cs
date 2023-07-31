// 日本語対応
#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// ステートマシンのノードエディタを表示するためのクラス
/// </summary>
public class StateMachineNodeView : UnityEditor.Experimental.GraphView.Node
{
    public Action<StateMachineNodeView> OnNodeSelected;

    private StateMachineNode _node;
    public StateMachineNode Node => _node;

    private Port _input;
    private Port _output;

    public Port Input => _input;
    public Port Output => _output;

    private ProgressBar _progressBar;
    private float _progressBarValue = 0f;

    public StateMachineNodeView(StateMachineNode node) : base(StateMachineEditor.FindUxml("NodeView"))
    {
        this._node = node;
        if (node.GetType() == typeof(EntryNode))
        {
            this.title = "Entry Node";
        }
        else
        {
            this.title = node.Name;
        }

        this.viewDataKey = node.GUID;

        style.left = node.Position.x;
        style.top = node.Position.y;

        CreateInputPorts();
        CreateOutputPorts();

        CreateProgressBar();

        _node.OnValueChanged += () =>
        {
            this.title = node.Name;
        };

        SetupClasses();
    }

    private void SetupClasses()
    {
        if (_node is EntryNode)
        {
            AddToClassList("EntryNode");
        }
        else
        {
            AddToClassList("NomalNode");
        }
    }
    private void CreateProgressBar()
    {
        _progressBar = this.Q<ProgressBar>();

        if (_progressBar != null)
        {
            _progressBar.value = _progressBarValue;
        }
    }

    private void CreateInputPorts()
    {
        if (Node is not EntryNode)
        {
            _input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        }

        if (_input != null)
        {
            _input.portName = "";
            inputContainer.Add(_input);
        }
    }

    private void CreateOutputPorts()
    {
        if (Node is EntryNode)
        {
            _output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
        }
        else
        {
            _output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
        }

        if (_output != null)
        {
            _output.portName = "";
            outputContainer.Add(_output);
        }
    }

    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);

        Undo.RecordObject(_node, "State Machine (Set Position)");

        _node.Position = new Vector2(newPos.xMin, newPos.y);

        EditorUtility.SetDirty(_node);
    }
    public override void OnSelected()
    {
        //Debug.Log(this.GetHashCode());
        base.OnSelected();
        OnNodeSelected?.Invoke(this);
    }
    public void UpdateState()
    {
        RemoveFromClassList("running");

        if (Application.isPlaying)
        {
            if (Node != null && Node.IsRunning)
            {
                AddToClassList("running");
                if (_progressBarValue > 100f) _progressBarValue = 0;
                else _progressBarValue += 10f;
            }
            else
            {
                _progressBarValue = 0f;
            }
            _progressBar.value = _progressBarValue;
        }
    }
}
#endif