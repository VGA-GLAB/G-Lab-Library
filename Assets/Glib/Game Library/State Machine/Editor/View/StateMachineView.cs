// 日本語対応
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

/// <summary>
/// ステートマシンを編集するためのウィンドウクラス
/// </summary>
public class StateMachineView : GraphView
{
    public Action<StateMachineNodeView> OnNodeSelected;
    public new class UxmlFactory : UxmlFactory<StateMachineView, GraphView.UxmlTraits> { }
    private StateMachineSO _stateMachine;

    public StateMachineView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StateMachineEditor.FindUss("StateMachineEditor"));
        styleSheets.Add(styleSheet);

        Undo.undoRedoPerformed += OnUndoRedo;
    }

    private void OnUndoRedo()
    {
        if (_stateMachine != null)
        {
            PopulateView(_stateMachine);
            AssetDatabase.SaveAssets();
        }
    }

    StateMachineNodeView FindNodeView(StateMachineNode node)
    {
        return GetNodeByGuid(node.GUID) as StateMachineNodeView;
    }
    internal void PopulateView(StateMachineSO stateMachine)
    {
        _stateMachine = stateMachine;
        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements); // 要素の削除を行う。
        graphViewChanged += OnGraphViewChanged;

        if (_stateMachine.EntryNode == null)
        {
            _stateMachine.EntryNode = _stateMachine.CreateNode(typeof(EntryNode)) as EntryNode;
            EditorUtility.SetDirty(_stateMachine);
            AssetDatabase.SaveAssets();
        }

        _stateMachine.Nodes.ForEach(node => CreateNodeView(node));

        _stateMachine.Nodes.ForEach(node =>
        {
            var children = _stateMachine.GetNextStates(node);
            foreach (var child in children)
            {
                StateMachineNodeView parentView = FindNodeView(node);
                StateMachineNodeView childView = FindNodeView(child);

                Edge edge = parentView.Output.ConnectTo(childView.Input);
                AddElement(edge);
            }
        });
    }

    // グラフに対して何らかの変更が行われた時に実行される。
    // https://docs.unity3d.com/ja/2019.4/ScriptReference/Experimental.GraphView.GraphView-graphViewChanged.html
    // https://docs.unity3d.com/ja/2019.4/ScriptReference/Experimental.GraphView.GraphViewChange.html
    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if (graphViewChange.elementsToRemove != null) // 要素が削除される時に実行される。
        {
            graphViewChange.elementsToRemove.ForEach(removeElem => // 全ての削除される要素を走査する。
            {
                // ノードが削除される場合。
                StateMachineNodeView nodeView = removeElem as StateMachineNodeView;
                if (nodeView != null)
                {
                    _stateMachine.DeleteNode(nodeView.Node);
                }

                // エッジが削除される場合。
                Edge edge = removeElem as Edge;
                if (edge != null)
                {
                    StateMachineNodeView outNode = edge.output.node as StateMachineNodeView;
                    StateMachineNodeView inNode = edge.input.node as StateMachineNodeView;
                    _stateMachine.Disconnect(outNode.Node, inNode.Node); // 接続状態を解除する。（切断する。）
                }
            });
        }

        if (graphViewChange.edgesToCreate != null) // エッジが作成されようとしている時に実行される。
        {
            graphViewChange.edgesToCreate.ForEach(edge =>
            {
                StateMachineNodeView outNode = edge.output.node as StateMachineNodeView;
                StateMachineNodeView inNode = edge.input.node as StateMachineNodeView;

                _stateMachine.TryConnect(outNode.Node, inNode.Node); // 接続を試みる。（接続に失敗した場合falseを返す。）
            });
        }

        return graphViewChange;
    }


    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node).ToList();
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        //base.BuildContextualMenu(evt);

        evt.menu.AppendAction($"Create Node", a => CreateNode());
    }
    void CreateNode()
    {
        if (_stateMachine == null) { UnityEngine.Debug.Log("StateMachineが選択されていません。"); return; }
        StateMachineNode node = _stateMachine.CreateNode(typeof(StateMachineNode));
        CreateNodeView(node);
    }
    void CreateNodeView(StateMachineNode node)
    {
        StateMachineNodeView nodeView = new StateMachineNodeView(node);
        nodeView.OnNodeSelected = OnNodeSelected;
        AddElement(nodeView);
    }

    public void UpdateNodeState()
    {
        foreach (var node in nodes)
        {
            StateMachineNodeView view = node as StateMachineNodeView;
            view?.UpdateState();
        }
    }

}
#endif