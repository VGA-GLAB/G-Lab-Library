// 日本語対応
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace StateMachine
{
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
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            if (_stateMachine.EntryNode == null)
            {
                _stateMachine.EntryNode = _stateMachine.CreateNode(typeof(EntryNode)) as EntryNode;
                EditorUtility.SetDirty(_stateMachine);
                AssetDatabase.SaveAssets();
            }

            _stateMachine.Nodes.ForEach(n => CreateNodeView(n));

            _stateMachine.Nodes.ForEach(n =>
            {
                var children = _stateMachine.GetNextStates(n);
                foreach (var c in children)
                {
                    StateMachineNodeView parentView = FindNodeView(n);
                    StateMachineNodeView childView = FindNodeView(c);

                    Edge edge = parentView.Output.ConnectTo(childView.Input);
                    AddElement(edge);
                }
            });
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    StateMachineNodeView nodeView = elem as StateMachineNodeView;
                    if (nodeView != null)
                    {
                        _stateMachine.DeleteNode(nodeView.Node);
                    }

                    Edge edge = elem as Edge;
                    if (edge != null)
                    {
                        StateMachineNodeView outNode = edge.output.node as StateMachineNodeView;
                        StateMachineNodeView inNode = edge.input.node as StateMachineNodeView;
                        _stateMachine.RemoveTo(outNode.Node, inNode.Node);
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    StateMachineNodeView outNode = edge.output.node as StateMachineNodeView;
                    StateMachineNodeView inNode = edge.input.node as StateMachineNodeView;

                    if (!_stateMachine.AddTo(outNode.Node, inNode.Node))
                    {
                        deleteEdges.Add(edge);
                    } // ノードの割り当てに失敗したらEdgeを破棄する。
                });
            }
            // エッジの削除を遅延させる
            EditorApplication.delayCall += () =>
            {
                deleteEdges.ForEach(edge => RemoveElement(edge));
                deleteEdges.Clear();
            };

            return graphViewChange;
        }

        List<Edge> deleteEdges = new List<Edge>();

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
            foreach (var n in nodes)
            {
                StateMachineNodeView view = n as StateMachineNodeView;
                view?.UpdateState();
            }
        }
    }
}
#endif