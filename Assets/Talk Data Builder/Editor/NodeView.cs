#if UNITY_EDITOR
// 日本語対応
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Glib.Talk;

public class NodeView : UnityEditor.Experimental.GraphView.Node
{
    public Action<NodeView> OnNodeSelecgted;

    private Glib.Talk.Node _node;
    private Port _input = null;
    private Port _output = null;

    public Glib.Talk.Node Node => _node;
    public Port Input => _input;
    public Port Output => _output;

    public NodeView(Glib.Talk.Node node)
    {
        _node = node;
        title = node.name;
        viewDataKey = node.GUID;

        style.left = node.Position.x;
        style.top = node.Position.y;

        CreateInputPorts();
        CreateOutputPorts();

        ApplyNodeValue(node);
        node.OnValidated += ApplyNodeValue;
    }

    private void ApplyNodeValue(Glib.Talk.Node node)
    {
        // RootNodeのタイトルは固定。
        if (node is not RootNode)
        {
            title = node.Title;
        }
    }

    private void CreateOutputPorts()
    {
        // RootNodeにはInputPortは必要ない。それ以外の全てのNodeにはInputPortが必要。
        if (_node is not RootNode)
        {
            _input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        }

        if (_input != null)
        {
            _input.portName = "";
            inputContainer.Add(_input);
        }
    }

    private void CreateInputPorts()
    {
        // RootNodeにはInputPortは必要ない。それ以外の全てのNodeにはInputPortが必要。
        if (_node is RootNode)
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

        _node.SetPosition(new Vector2(newPos.xMin, newPos.yMin));
        _node.SortNextNodesByPositionY();

    }
    public override void OnSelected()
    {
        base.OnSelected();

        OnNodeSelecgted?.Invoke(this);
    }
}
#endif