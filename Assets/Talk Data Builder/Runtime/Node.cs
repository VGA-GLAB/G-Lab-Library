// 日本語対応
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Glib
{
    namespace Talk
    {
        public class Node : ScriptableObject
        {
            [SerializeField]
            private string _title = "title ";
            [SerializeField, TextArea(0, 1000), FormerlySerializedAs("Text")]
            private string _text;
            [SerializeField]
            [HideInInspector]
            private List<Node> _nextNodes = new List<Node>();
            [SerializeField]
            [HideInInspector]
            private Vector2 _position;
            [SerializeField]
            [HideInInspector]
            private string _guid;

            public string GUID { get => _guid; set => _guid = value; }
            public Vector2 Position => _position;

            public string Title => _title;
            public string Text => _text;
            public List<Node> NextNodes => _nextNodes;

            public event Action<Node> OnValidated;

            private void OnValidate()
            {
                OnValidated?.Invoke(this);
            }
            // エディタ上で編集するモノをオリジンとする。
            // ランタイムでは実行開始時に複製し、その後は複製を取り扱う。
            public Node Clone()
            {
                return Instantiate(this);
            }
            public void RuntimeInitialize(Node origin, Dictionary<Node, Node> originToClone)
            {
                // 自身をクローンとして取り扱う。
                // _nextNodesリストも複製に変換する。
                this._nextNodes.Clear();
                foreach (Node nextOrigin in origin._nextNodes)
                {
                    var nextClone = originToClone[nextOrigin];
                    this._nextNodes.Add(nextClone);
                }
            }
            public void SetPosition(Vector2 pos)
            {
                this._position = pos;
            }

            public event Action OnHovered;
            public event Action OnUnhovered;
            public event Action OnSelected;

            private bool _isHovered = false;
            private bool _isSelected = false;

            public bool IsHovered => _isHovered;
            public bool IsSelected => _isSelected;

            public void Hover()
            {
                _isHovered = true;
                OnHovered?.Invoke();
            }
            public void Unhover()
            {
                _isHovered = false;
                OnUnhovered?.Invoke();
            }
            public void Select()
            {
                _isSelected = true;
                OnSelected?.Invoke();
            }

            public void SortNextNodesByPositionY()
            {
                // _nextNodesリストを_position.yの値でソート
                _nextNodes.Sort((node1, node2) => node1._position.y.CompareTo(node2._position.y));
            }
        }
    }
}