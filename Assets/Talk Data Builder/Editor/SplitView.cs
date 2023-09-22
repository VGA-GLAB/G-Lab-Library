#if UNITY_EDITOR
// 日本語対応
using UnityEngine.UIElements;

public class SplitView : TwoPaneSplitView
{
    public new class UxmlFactory : UxmlFactory<SplitView, TwoPaneSplitView.UxmlTraits> { }
}
#endif