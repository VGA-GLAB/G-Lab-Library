#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[CustomPropertyDrawer(typeof(AnimationParameterAttribute))]
public class AnimationParameterPropertyDrawer : PropertyDrawer
{
    private int _index = -1;
    private GUIContent[] _guiContents = default;
    private bool _isSuccessSetUp = default;
    private string[] _animParams;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var oldstrings = _animParams; // アニメーションコントローラーのパラメータに変化があれば再セットアップする。
        _animParams = GetElements(property);
        if (_animParams == null) return;
        if (_index == -1 || !_animParams.Equals(oldstrings) || !_isSuccessSetUp)
        {
            _isSuccessSetUp = SetUp(position, property);
        }
        if (_isSuccessSetUp)
        {
            int oldIndex = _index;
            // ユーザーの入力を取得する。
            _index = EditorGUI.Popup(position, label, _index, _guiContents);

            if (oldIndex != _index) // 前に選択していたモノと変更があれば更新する。
            {
                property.stringValue = _guiContents[_index].text;
            }
        }
    }

    private bool SetUp(Rect position, SerializedProperty property)
    {
        // アニメーターを取得
        Animator animator = (property.serializedObject.targetObject as Component).GetComponent<Animator>();
        if (animator == null)
        {
            EditorGUI.LabelField(position, "AnimationParameter属性が使用されていますが、AnimatorComponentがアタッチされていません");
            return false; // 取得に失敗したら処理を終了する。
        }

        // アニメーションコントローラを取得
        AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
        if (animatorController == null)
        {
            EditorGUI.LabelField(position, "AnimationParameter属性が使用されていますが、AnimatorControllerがアサインされていません。");
            return false;
        }

        // パラメータ名の配列を作成
        string[] parameterNames = animatorController.parameters.Select(p => p.name).ToArray();
        if (parameterNames.Length == 0)
        {
            EditorGUI.LabelField(position, "AnimationParameter属性が使用されていますが、parameter が一つも設定されていません。");
            return false;
        }

        // GUIに描画するための空の配列を作成
        _guiContents = new GUIContent[parameterNames.Length];

        // 空の配列に値を割り当てる。
        for (int i = 0; i < parameterNames.Length; i++)
        {
            _guiContents[i] = new GUIContent($"{parameterNames[i]}");
        }

        // 再設定用プログラム
        if (!string.IsNullOrEmpty(property.stringValue))
        {
            bool animNameFound = false;
            for (int i = 0; i < _guiContents.Length; i++)
            {
                if (_guiContents[i].text == property.stringValue)
                {
                    _index = i;
                    animNameFound = true;
                    break;
                }
            }
            if (!animNameFound)
                _index = 0;
        }
        else _index = 0;

        // プロパティに値を設定する。
        property.stringValue = _guiContents[_index].text;

        return true;
    }

    private string[] GetElements(SerializedProperty property)
    {
        // アニメーターを取得
        Animator animator = (property.serializedObject.targetObject as Component).GetComponent<Animator>();
        if (animator == null)
        {
            return null;
        }

        // アニメーションコントローラを取得
        AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
        if (animatorController == null)
        {
            return null;
        }

        // パラメータ名の配列を作成
        return animatorController.parameters.Select(p => p.name).ToArray();
    }
}
#endif