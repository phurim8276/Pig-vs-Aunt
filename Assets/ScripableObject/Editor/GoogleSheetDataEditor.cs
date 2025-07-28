using UnityEditor;
using UnityEngine;
using System.Net;

[CustomEditor(typeof(GoogleSheetData))]
public class GoogleSheetDataEditor : Editor
{
    private string sheetID = "YourSheetID";
    private string gidID = "0";
    private ItemDatabase targetDatabase;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GoogleSheetData data = (GoogleSheetData)target;

        GUILayout.Space(10);
        GUILayout.Label("Google Sheet Settings", EditorStyles.boldLabel);

        sheetID = EditorGUILayout.TextField("Sheet ID", sheetID);
        gidID = EditorGUILayout.TextField("Gid ID", gidID);

        GUILayout.Space(10);
        GUILayout.Label("Export To", EditorStyles.boldLabel);
        targetDatabase = (ItemDatabase)EditorGUILayout.ObjectField("Target Database", targetDatabase, typeof(ItemDatabase), false);

        if (GUILayout.Button("Load Data from Google Sheet"))
        {
            LoadData(data);

            

            if (targetDatabase != null)
            {
                LoadIntoDatabase(data, targetDatabase);
            }
            else
            {
                Debug.LogError("Please assign an ItemDatabase asset.");
            }
        }

        
    }

    private void LoadData(GoogleSheetData data)
    {
        string url = $"https://docs.google.com/spreadsheets/d/{sheetID}/export?format=csv&gid={gidID}";

        using (WebClient client = new WebClient())
        {
            try
            {
                data.rawCsvData = client.DownloadString(url);
                data.ParseCSV();

                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssets();
                Debug.Log("Google Sheet data loaded successfully!");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error loading Google Sheet data: " + e.Message);
            }
        }
    }

    private void LoadIntoDatabase(GoogleSheetData data, ItemDatabase database)
    {
        ItemData itemData = GoogleSheetParser.ConvertToItemData(data);
        if (itemData != null)
        {
            database.data = itemData;
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            Debug.Log("ItemDatabase updated with new data from Google Sheet.");
        }
        else
        {
            Debug.LogWarning("No data was found in the Google Sheet.");
        }
    }
}
