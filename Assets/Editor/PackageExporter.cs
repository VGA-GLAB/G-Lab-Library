// 日本語対応
using System.Collections.Generic;
using UnityEditor;

public class PackageExporter
{
    private static readonly string PackagePath = "Assets/Glib/";

    [MenuItem("Tools/ExportPackage")]
    // 必ずstaticにする
    private static void Export()
    {
        // 出力ファイル名
        var exportPath = "./GlibUnityPackage.unitypackage";

        var exportedPackageAssetList = new List<string>();
        foreach (var guid in AssetDatabase.FindAssets("", new[] { PackagePath }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            exportedPackageAssetList.Add(path);
        }

        AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(),
            exportPath,
            ExportPackageOptions.Recurse);
    }
}
