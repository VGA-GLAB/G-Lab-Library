// 日本語対応
using UnityEngine;
using Glib.Talk;
using System.Collections.Generic;

public class SelectableTextController : MonoBehaviour
{
    [SerializeField]
    private Selectable _selectablePrefab;
    [SerializeField]
    private Transform _selectableParent;

    private readonly List<Selectable> _selectableList = new List<Selectable>();

    public void ApplyText(IReadOnlyList<Node> nodes)
    {
        Clear();

        foreach (var node in nodes)
        {
            var selectable = GameObject.Instantiate(_selectablePrefab, _selectableParent);
            selectable.Initialize(node);
            _selectableList.Add(selectable);
        }
    }

    public void Clear()
    {
        foreach (var selectable in _selectableList)
        {
            selectable.Dispose();
            GameObject.Destroy(selectable.gameObject);
        }
        _selectableList.Clear();
    }
}