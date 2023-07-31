// 日本語対応
#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace StateMachine
{
    public class SplitView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SplitView, TwoPaneSplitView.UxmlTraits> { }
    }
}
#endif