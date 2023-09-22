// 日本語対応
using UnityEngine;
using Glib.Talk;
using UnityEngine.UI;

public class Selectable : MonoBehaviour
{
    [SerializeField]
    private Image _image;
    [SerializeField]
    private Text _text;
    [SerializeField]
    private Color _nomalColor = Color.white;
    [SerializeField]
    private Color _hoverColor = Color.blue;

    private Node _node;

    public void Initialize(Node node)
    {
        _node = node;
        _text.text = node.Text;

        if (node.IsHovered)
        {
            Hover();
        }
        else
        {
            Unhover();
        }

        node.OnHovered += Hover;
        node.OnUnhovered += Unhover;
    }

    public void Dispose()
    {
        _node.OnHovered -= Hover;
        _node.OnUnhovered -= Unhover;
    }

    public void Hover()
    {
        _image.color = _hoverColor;
    }

    public void Unhover()
    {
        _image.color = _nomalColor;
    }

    public void Select()
    {

    }
}