// 日本語対応
using UnityEngine;
using UnityEngine.UI;

public class MessageText : MonoBehaviour
{
    [SerializeField]
    private Text _text;

    public void SetText(string text)
    {
        _text.text = text;
    }

    public void Clear()
    {
        _text.text = null;
    }
}