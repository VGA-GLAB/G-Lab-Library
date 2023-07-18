#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorDrawer : PropertyDrawer
{
    private int _currentIndex = -1;
    private string[] _typeFullNameArray;
    private Type[] _inheritedTypes = null;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ManagedReference) return;
        if (_currentIndex == -1) Setup(property);

        int oldIndex = _currentIndex;

        // なぜか これがないと表示がバグる。
        _currentIndex = Array.IndexOf(_typeFullNameArray, property.managedReferenceFullTypename);
        if (_currentIndex == -1) _currentIndex = 0;

        _currentIndex = EditorGUI.Popup(position, label.text, _currentIndex, _typeFullNameArray);

        if (_currentIndex != oldIndex)
        {
            SelectedIndexChanged(property);
        }
    }

    /// <summary> メンバの初期化処理 </summary>
    private void Setup(SerializedProperty property)
    {
        Type propertyType = GetFieldType(property);
        Type monoType = typeof(MonoBehaviour);
        // 継承されたクラスの配列を取得する。
        _inheritedTypes = AppDomain.CurrentDomain.GetAssemblies() // 実行環境のアセンブリを取得
            .SelectMany(s => s.GetTypes()) // アセンブリに存在する型を取得
            .Where(p => propertyType.IsAssignableFrom(p) && p.IsClass && !monoType.IsAssignableFrom(p))// propertyの型がpの型に割り当て可能であるか && pはクラスであるか && pはMonoBehaviourを継承してないか？ 
            .Prepend(null) // シーケンスの先頭にnullを追加します。
            .ToArray();
        // クラス名を配列でとっておく。
        _typeFullNameArray =
            _inheritedTypes.Select(type => type == null ? "<null>" : string.Format("{0} {1}", type.Assembly.ToString().Split(',')[0], type.FullName)).ToArray();

        // 選択されていた値を検索し見つかった場合、インデックスを再指定する。
        // 再コンパイルが走るとなぜか参照が外れるので、外れ防止用。
        if (property.managedReferenceValue != null)
        {
            bool found = false;
            for (int i = 0; i < _inheritedTypes.Length; i++)
            {
                if (_inheritedTypes[i] == property.managedReferenceValue.GetType())
                {
                    _currentIndex = i;
                    found = true;
                    break;
                }
            }
            if (!found) _currentIndex = 0;
        }
        else _currentIndex = 0;
    }

    /// <summary> シリアライズ対象の型をType型で返す。 </summary>
    public static Type GetFieldType(SerializedProperty property)
    {
        const BindingFlags bindingAttr =
                  BindingFlags.NonPublic |
                  BindingFlags.Public |
                  BindingFlags.FlattenHierarchy |
                  BindingFlags.Instance;

        var propertyPaths = property.propertyPath.Split('.');
        var fieldType = property.serializedObject.targetObject.GetType(); // シリアライズ元のクラスをType型でとってくる。
        for (int i = 0; i < propertyPaths.Length; i++)
        {
            FieldInfo field = fieldType.GetField(propertyPaths[i], bindingAttr);
            // 配列対応
            if (propertyPaths[i].Contains("Array"))
            {
                // 配列の場合
                if (fieldType.IsArray)
                {
                    // GetElementType で要素の型を取得する
                    fieldType = fieldType.GetElementType();
                }
                // リストの場合
                else
                {
                    // GetGenericArguments で要素の型を取得する
                    var genericArguments = fieldType.GetGenericArguments();
                    fieldType = genericArguments[0];
                }

                //data[0]を評価しに行くので、とばす。
                ++i;
                continue;
            }
            else
                fieldType = field.FieldType;
        }

        return fieldType;
    }
    // 選択しているオブジェクトに変更があった場合の処理
    private void SelectedIndexChanged(SerializedProperty property)
    {
        try
        {
            Type selectedType = _inheritedTypes[_currentIndex];
            var instance = Activator.CreateInstance(selectedType);

            property.managedReferenceValue = selectedType == null ? null : instance;
        }
        catch (MissingMethodException)
        {
            // https://learn.microsoft.com/ja-jp/dotnet/api/system.activator.createinstance?view=net-7.0#:~:text=CreateInstance(Type)-,%E6%8C%87%E5%AE%9A%E3%81%95%E3%82%8C%E3%81%9F%E5%9E%8B%E3%81%AE%E3%83%91%E3%83%A9%E3%83%A1%E3%83%BC%E3%82%BF%E3%83%BC%E3%81%AA%E3%81%97%E3%81%AE%E3%82%B3%E3%83%B3%E3%82%B9%E3%83%88%E3%83%A9%E3%82%AF%E3%82%BF%E3%83%BC%E3%82%92%E4%BD%BF%E7%94%A8%E3%81%97%E3%81%A6%E3%80%81%E6%8C%87%E5%AE%9A%E3%81%95%E3%82%8C%E3%81%9F%E5%9E%8B%E3%81%AE%E3%82%A4%E3%83%B3%E3%82%B9%E3%82%BF%E3%83%B3%E3%82%B9%E3%82%92%E4%BD%9C%E6%88%90%E3%81%97%E3%81%BE%E3%81%99%E3%80%82,-CreateInstance%3CT%3E()
            Debug.Log("引数なしのコンストラクタが見つかりませんでした。");
        }
        catch (ArgumentNullException)
        {
            property.managedReferenceValue = null;
        }
        catch (IndexOutOfRangeException)
        {

        }
    }
}
#endif