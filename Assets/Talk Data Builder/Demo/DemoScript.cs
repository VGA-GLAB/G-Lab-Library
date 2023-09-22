// 日本語対応
using UnityEngine;

public class DemoScript : MonoBehaviour
{
    [SerializeField]
    private SelectableTextController _selectableTextController;
    [SerializeField]
    private MessageText _messageText;

    private TalkDataController _talkController;

    private void Start()
    {
        _talkController = GetComponent<TalkDataController>();
        ApplyText();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Debug.Log("下");
            _talkController.NextInput();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log("上");
            _talkController.PrevInput();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            _talkController.Step();
            ApplyText();
        }
    }

    private void ApplyText()
    {
        // 表示を更新する。
        // 古いメッセージや選択肢を破棄する。
        _messageText.Clear();
        _selectableTextController.Clear();
        // 新しいCurrentが一つであればメッセージに表示する。
        // 新しいCurrentが複数あれば選択肢表示する。
        // どちらの項にも当てはまらない場合何もしない。
        if (_talkController.Currents != null)
        {
            if (_talkController.Currents.Count == 1)
            {
                _messageText.SetText(_talkController.Currents[0].Text);
            }
            else if (_talkController.Currents.Count >= 2)
            {
                _selectableTextController.ApplyText(_talkController.Currents);
            }
        }
    }
}