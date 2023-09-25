// 日本語対応
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Glib
{
    namespace DataLoader
    {

        public static class GoogleSheetsLoader
        {
            public static IEnumerator LoadGoogleSheets(string sheetID, string sheetName, Action<List<string[]>> onDataLoaded, int ignoreRows = 0)
            {
                // 参考リンク: https://qiita.com/simanezumi1989/items/32436230dadf7a123de8
                // sheetIDの指定方法: https://qiita.com/simanezumi1989/items/32436230dadf7a123de8#:~:text=%E7%94%BB%E5%83%8F1-,SHEET_ID%E3%81%AF%E4%BB%A5%E4%B8%8B%E3%81%AE,-https%3A//docs.google
                // sheetNameの指定方法: https://qiita.com/simanezumi1989/items/32436230dadf7a123de8#:~:text=%E7%94%BB%E5%83%8F2-,SHEET_NAME%E3%81%AF%E4%BB%A5%E4%B8%8B%E3%81%AE%E6%96%87%E5%AD%97%E5%88%97,-%E5%BF%9C%E7%94%A8%EF%BC%88%E3%81%8A%E5%8C%96%E7%B2%A7%E7%9B%B4%E3%81%97
                // out:csvのところはカスタマイズ可能っぽい。 https://dokuro.moe/unity-how-to-import-google-spreadsheet-data-directly/#:~:text=tqx%20%E3%81%8C%E4%BD%95,%E3%81%A9%E3%81%86%E3%81%AA%E3%82%8B%E3%81%AE%EF%BC%9F

                var url = "https://docs.google.com/spreadsheets/d/" + sheetID + "/gviz/tq?tqx=out:csv&sheet=" + sheetName;
                List<string[]> data = new List<string[]>();

                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    yield return request.SendWebRequest();

                    var http_error = request.result == UnityWebRequest.Result.ProtocolError;
                    var network_error = request.result == UnityWebRequest.Result.ConnectionError;

                    if (http_error || network_error)
                    {
                        Debug.LogError("Error loading data from Google Sheets: " + request.error);
                    }
                    else
                    {
                        string responseText = request.downloadHandler.text;
                        string[] lines = responseText.Split('\n');

                        for (int i = ignoreRows; i < lines.Length; i++)
                        {
                            string[] rowData = lines[i].Split(',');
                            for (int j = 0; j < rowData.Length; j++)
                                rowData[j] = rowData[j].Trim('"'); ;
                            data.Add(rowData);
                        }
                    }

                    onDataLoaded?.Invoke(data);
                }
            }
        }
    }
}