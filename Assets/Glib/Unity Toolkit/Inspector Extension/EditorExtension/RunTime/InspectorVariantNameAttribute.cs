using System;
using UnityEngine;

/// <summary>
/// エディタ拡張で変数名を指定できる
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class InspectorVariantName : PropertyAttribute
{
    string _variantName = "";
    public string VariantName => _variantName;

    public InspectorVariantName(string name)
    {
        _variantName = name;
    }
}
