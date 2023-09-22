#if UNITY_EDITOR
// 日本語対応
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System;
using Glib.Talk;

public class TalkDataEditorView : GraphView
{
    public Action<NodeView> OnNodeSelected;
    public new class UxmlFactory : UxmlFactory<TalkDataEditorView, GraphView.UxmlTraits> { }

    private TalkDataBuilder _tree;

    public TalkDataEditorView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(TalkDataEditor.FindUss("TalkDataEditor"));
        styleSheets.Add(styleSheet);
    }

    NodeView FindNodeView(Glib.Talk.Node node)
    {
        return GetNodeByGuid(node.GUID) as NodeView;
    }

    internal void PopulateView(TalkDataBuilder tree)
    {
        _tree = tree;

        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        if (tree.RootNode == null)
        {
            tree.RootNode = tree.CreateRootNode();
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();
        }

        tree.Nodes.ForEach(n => CreateNodeView(n));

        tree.Nodes.ForEach(n =>
        {
            var children = tree.GetChildren(n);

            foreach (var c in children)
            {
                NodeView parentView = FindNodeView(n);
                NodeView childView = FindNodeView(c);

                Edge edge = parentView.Output.ConnectTo(childView.Input);
                AddElement(edge);
            }
        });
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node).ToList();
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if (graphViewChange.elementsToRemove != null)
        {
            graphViewChange.elementsToRemove.ForEach(elem =>
            {
                NodeView nodeView = elem as NodeView;
                if (nodeView != null)
                {
                    _tree.DeleteNode(nodeView.Node);
                }

                Edge edge = elem as Edge;
                if (edge != null)
                {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;
                    _tree.RemoveChild(parentView.Node, childView.Node);
                }
            });
        }

        if (graphViewChange.edgesToCreate != null)
        {
            graphViewChange.edgesToCreate.ForEach(elem =>
            {
                NodeView parentView = elem.output.node as NodeView;
                NodeView childView = elem.input.node as NodeView;

                _tree.AddChild(parentView.Node, childView.Node);
            });
        }

        return graphViewChange;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        // base.BuildContextualMenu(evt);
        {
            evt.menu.AppendAction($"[Create Node]", (a) => CreateNode());
        }
    }

    void CreateNode()
    {
        Glib.Talk.Node node = _tree.CreateNode();
        CreateNodeView(node);
    }

    void CreateNodeView(Glib.Talk.Node node)
    {
        NodeView nodeView = new NodeView(node);
        EditorUtility.SetDirty(node);
        nodeView.OnNodeSelecgted = OnNodeSelected;
        AddElement(nodeView);
    }
}
#endif