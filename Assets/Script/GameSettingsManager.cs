using UnityEngine;

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance { get; private set; }

    [Header("Game Mode")]
    public bool isSinglePlayer = false;

    [Header("Difficulty")]
    public string difficulty = "normal";

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    public void SetGameMode(bool singlePlayer)
    {
        isSinglePlayer = singlePlayer;
        Debug.Log("Game Mode set to: " + (singlePlayer ? "1 Player" : "2 Player"));
    }

    public void SetDifficulty(string level)
    {
        difficulty = level;
        Debug.Log("Difficulty set to: " + level);
    }
}
