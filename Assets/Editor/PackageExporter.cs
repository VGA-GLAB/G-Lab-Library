// 日本語対応
using System.Collections.Generic;
using UnityEditor;

public class PackageExporter
{
    // packageにしたいフォルダのパス
    private static readonly string PackagePath = "Assets/Glib/";

    [MenuItem("Tools/ExportPackage")]
    // ワークフローから呼び出すため 必ずstaticにする
    private static void Export()
    {
        // 出力ファイル名
        var exportPath = "./GlibUnityPackage.unitypackage";

        // packageにする処理。
        // アセットを検索し、guidを取得する。
        var exportedPackageAssetList = new List<string>();
        foreach (var guid in AssetDatabase.FindAssets("", new[] { PackagePath }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            exportedPackageAssetList.Add(path);
        }
        // package化処理。（下記の関数に渡すだけで勝手にやってくれる。）
        AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(),
            exportPath,
            ExportPackageOptions.Recurse);
    }
}
