// 日本語対応
using System.Collections.Generic;
using UnityEngine;

namespace Glib
{
    namespace Talk
    {
        [CreateAssetMenu(fileName = "Talk Data Builder", menuName = "Glib/Talk Data Builder")]
        public class TalkDataBuilder : ScriptableObject
        {
            [SerializeField]
            private RootNode _rootNode;
            [SerializeField]
            private List<Node> _nodes = new List<Node>();

            public RootNode RootNode { get => _rootNode; set => _rootNode = value; }
            public List<Node> Nodes => _nodes;

#if UNITY_EDITOR
            public Node CreateNode()
            {
                Node node = ScriptableObject.CreateInstance<Node>();
                node.name = "Talk Data";
                node.GUID = UnityEditor.GUID.Generate().ToString();
                _nodes.Add(node);

                node.SortNextNodesByPositionY();

                UnityEditor.AssetDatabase.AddObjectToAsset(node, this);
                UnityEditor.AssetDatabase.SaveAssets();
                return node;
            }
            public RootNode CreateRootNode()
            {
                RootNode data = ScriptableObject.CreateInstance<RootNode>();
                data.name = "Root Node";
                data.GUID = UnityEditor.GUID.Generate().ToString();
                _nodes.Add(data);

                UnityEditor.AssetDatabase.AddObjectToAsset(data, this);
                UnityEditor.AssetDatabase.SaveAssets();
                return data;
            }
            public void DeleteNode(Node node)
            {
                _nodes.Remove(node);
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(node);
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif
            public void AddChild(Node parent, Node child)
            {
                if (parent is RootNode)
                {
                    parent.NextNodes.Clear();
                }
                parent.NextNodes.Add(child);
            }
            public void RemoveChild(Node parent, Node child)
            {
                parent.NextNodes.Remove(child);
            }
            public List<Node> GetChildren(Node parent)
            {
                return parent.NextNodes;
            }
            public TalkDataBuilder Clone()
            {
                var clone = new TalkDataBuilder();
                var cloneNodes = new List<Node>();
                var originToCloneNode = new Dictionary<Node, Node>();

                foreach (var originNode in _nodes)
                {
                    var cloneNode = originNode.Clone();
                    cloneNodes.Add(cloneNode);
                    originToCloneNode.Add(originNode, cloneNode);
                }

                clone.RootNode = originToCloneNode[this.RootNode] as RootNode;

                foreach (var node in originToCloneNode)
                {
                    var originNode = node.Key;
                    var cloneNode = node.Value;
                    cloneNode.RuntimeInitialize(originNode, originToCloneNode);
                }

                clone._nodes = cloneNodes;
                return clone;
            }
        }
    }
}