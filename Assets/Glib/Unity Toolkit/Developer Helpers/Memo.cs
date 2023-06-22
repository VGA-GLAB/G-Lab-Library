// 日本語対応
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// メモ用コンポーネント
/// </summary>
public class Memo : MonoBehaviour
{
#if UNITY_EDITOR
#pragma warning disable 0414
    // 上記は、使用していない変数の警告を削除する為のプリプロセッサディレクティブ
    [Tooltip("このゲームオブジェクトに関するメモ"), TextArea(0, 100), SerializeField, FormerlySerializedAs("Text")]
    private string _text = default;
#endif
}
