#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(InputNameAttribute))]
public class InputName : PropertyDrawer
{
    /// <summary> 選択中のインデックス </summary>
    private int _index = -1;
    /// <summary> ドロップボックスに表示する値 </summary>
    private GUIContent[] _inputNames;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // セットアップが未だ完了していなければ実行する。
        if (_index == -1)
            SetUp(property);

        // 選択中の値が変更されたかどうか判断するために一時保存しておく
        int oldIndex = _index;
        // 選択されたインデックスを取得する
        _index = EditorGUI.Popup(position, label, _index, _inputNames);

        // 選択中のインデックスが変更された場合、プロパティの値を変更する。
        if (oldIndex != _index)
        {
            property.stringValue = _inputNames[_index].text;
        }
    }

    void SetUp(SerializedProperty property)
    {
        // アセットファイルを読み込む。
        var inputManager = AssetDatabase.LoadAssetAtPath("ProjectSettings/InputManager.asset", typeof(UnityEngine.Object));
        // スクリプトから変更可能なようにする。
        var serializedObject = new SerializedObject(inputManager);
        // ターゲットのオブジェクトから特定のプロパティを検索/取得する。
        var axesProperty = serializedObject.FindProperty("m_Axes");

        // インプットマネージャーに登録されたNameを取得/保存する。
        var names = new List<string>();
        for (int i = 0; i < axesProperty.arraySize; i++)
        {
            // 特定のインデックスのプロパティを取得
            var axis = axesProperty.GetArrayElementAtIndex(i);
            // 取得したプロパティの文字列を取得
            var name = axis.FindPropertyRelative("m_Name").stringValue;
            // リストに追加
            names.Add(name);
        }

        // インスペクタウィンドウに表示するためのコンテントを作成
        _inputNames = new GUIContent[names.Count]; // 配列を作って
        for (int i = 0; i < names.Count; i++)
        {
            _inputNames[i] = new GUIContent($"{names[i]}"); // 中身を割り当てる
        }

        // 選択されていた値を検索し見つかった場合、インデックスを再指定する。
        // (この処理はエディター実行時に再コンパイルされ初期化されてしまうので必要な処理)
        // 見つからなかった場合インデックスは0で初期化する。
        if (!string.IsNullOrEmpty(property.stringValue))
        {
            bool inputNameFound = false;
            for (int i = 0; i < _inputNames.Length; i++)
            {
                if (_inputNames[i].text == property.stringValue)
                {
                    _index = i;
                    inputNameFound = true;
                    break;
                }
            }
            if (!inputNameFound)
                _index = 0;
        }
        else _index = 0;

        // プロパティに値を設定する。
        property.stringValue = _inputNames[_index].text;
    }
}
#endif