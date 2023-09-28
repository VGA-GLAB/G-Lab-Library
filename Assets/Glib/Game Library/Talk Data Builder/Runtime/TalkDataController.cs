// 日本語対応
using Glib.Talk;
using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class TalkDataController : MonoBehaviour
{
    [SerializeField]
    private TalkDataBuilder _talkData;

    private int _index = 0;
    private IReadOnlyList<Node> _currents = null;

    public TalkDataBuilder TalkData => _talkData;
    public int Index => _index;
    public IReadOnlyList<Node> Currents => _currents;

    public Node HoverItem => _currents[_index];

    private void Start()
    {
        _talkData = _talkData.Clone();
        _currents = _talkData.RootNode.NextNodes;
    }

    public void ResetCurrents()
    {
        _currents = _talkData.RootNode.NextNodes;
    }

    public void NextInput()
    {
        // 必要であればIndexを更新する。
        if (_currents != null && _currents.Count >= 2)
        {
            HoverItem.Unhover();
            _index++;
            if (_currents.Count == _index) _index = 0;
            HoverItem.Hover();
        }
    }
    public void PrevInput()
    {
        // 必要であればIndexを更新する。
        if (_currents != null && _currents.Count >= 2)
        {
            HoverItem.Unhover();
            _index--;
            if (_index < 0) _index = _currents.Count - 1;
            HoverItem.Hover();
        }
    }
    public void Step()
    {
        // _currentsを更新する。
        if (_currents != null &&
            IsInIndex(_currents, _index) &&
            _currents[_index].NextNodes != null &&
            _currents[_index].NextNodes.Count != 0)
        {
            HoverItem.Unhover();
            _currents = _currents[_index].NextNodes;
            _index = 0;
            HoverItem.Hover();
        }
        else
        {
            Debug.LogWarning("既にリーフノードだと思われます。");
        }
    }

    public bool TryStep(out IReadOnlyList<Node> nexts)
    {
        // _currentsを更新する。
        if (_currents != null &&
            IsInIndex(_currents, _index) &&
            _currents[_index].NextNodes != null &&
            _currents[_index].NextNodes.Count != 0)
        {
            HoverItem.Unhover();
            _currents = _currents[_index].NextNodes;
            _index = 0;
            HoverItem.Hover();

            nexts = _currents;
            return true;
        }
        else
        {
            Debug.LogWarning("既にリーフノードだと思われます。");
            nexts = null;
            return false;
        }
    }
    private bool IsInIndex(Array array, int index)
    {
        return index >= 0 && index < array.Length;
    }
    private bool IsInIndex<T>(IReadOnlyList<T> list, int index)
    {
        return index >= 0 && index < list.Count;
    }
}