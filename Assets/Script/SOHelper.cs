using UnityEngine;

public static class SOHelper
{
    /// <summary>
    /// Load a ScriptableObject of type T from the Resources folder by name.
    /// </summary>
    public static T LoadSO<T>(string assetName) where T : ScriptableObject
    {
        T so = Resources.Load<T>(assetName);
        if (so == null)
        {
            Debug.LogError($"[SOHelper] ScriptableObject '{assetName}' not found in Resources!");
        }
        return so;
    }

    /// <summary>
    /// Load a ScriptableObject of type T from a subfolder in Resources.
    /// Example: "Data/MyData"
    /// </summary>
    public static T LoadSOFromPath<T>(string path) where T : ScriptableObject
    {
        T so = Resources.Load<T>(path);
        if (so == null)
        {
            Debug.LogError($"[SOHelper] ScriptableObject '{path}' not found in Resources!");
        }
        return so;
    }
}
