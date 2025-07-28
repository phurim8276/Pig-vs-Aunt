using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GoogleSheetData", menuName = "Data/Google Sheet Data")]
public class GoogleSheetData : ScriptableObject
{
    [TextArea]
    public string rawCsvData;

    [Serializable]
    public class Entry
    {
        public string key;
        public string value;
    }

    [Serializable]
    public class Row
    {
        public List<string> cells = new List<string>();
    }

    public List<Row> parsedRows = new List<Row>();

    public void ParseCSV()
    {
        parsedRows.Clear();

        if (string.IsNullOrEmpty(rawCsvData))
            return;

        string[] lines = rawCsvData.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            string[] split = line.Split(',');
            Row row = new Row();
            foreach (var cell in split)
            {
                row.cells.Add(cell.Trim().Trim('"')); // Clean spaces and quotes
            }
            parsedRows.Add(row);
        }
    }


    /// <summary>
    /// Convert parsedRows to a list of dictionaries using the first row as headers.
    /// </summary>
    public List<Dictionary<string, string>> ToJsonObjects()
    {
        var jsonObjects = new List<Dictionary<string, string>>();
        if (parsedRows.Count < 2) return jsonObjects;

        var headers = parsedRows[0].cells;
        for (int i = 1; i < parsedRows.Count; i++)
        {
            var dict = new Dictionary<string, string>();
            for (int j = 0; j < headers.Count && j < parsedRows[i].cells.Count; j++)
            {
                dict[headers[j]] = parsedRows[i].cells[j];
            }
            jsonObjects.Add(dict);
        }

        return jsonObjects;
    }
    public string ToJson<T>(List<T> dataList)
    {
        return JsonUtility.ToJson(new Wrapper<T>(dataList), true);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> Items;
        public Wrapper(List<T> items)
        {
            Items = items;
        }
    }

}
