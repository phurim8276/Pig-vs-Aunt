using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // For loading game scene

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject chooseGamePanel;
    public GameObject difficultyPanel;
    public GameObject loginPanel;
    public GameObject howToPlayPanel;

    private bool isSinglePlayer = false;

    public Button logoutButton;
    public Button loginButton;

    void Start()
    {
        // Ensure correct initial state
        mainMenuPanel.SetActive(false);
        chooseGamePanel.SetActive(false);
        difficultyPanel.SetActive(false);
        loginPanel.SetActive(true);

       
    }

        // ---------------- MENU FLOW ----------------

    public void OnStartButton()
    {
        mainMenuPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        chooseGamePanel.SetActive(true);
    }

    public void OnSelect1Player()
    {
        GameSettingsManager.Instance.SetGameMode(true);
        difficultyPanel.SetActive(true);
    }

    public void OnSelect2Player()
    {
        GameSettingsManager.Instance.SetGameMode(false);
        StartGame("normal");
    }

    public void OnHowToPlay()
    {
        mainMenuPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
    }
    public void TriggerLogButton()
    {
        LoginWithGoogle loginWithGoogle = FindObjectOfType<LoginWithGoogle>();
        if (loginWithGoogle != null)
        {

            if (loginWithGoogle.isGuestLogin)
            {
                loginButton.gameObject.SetActive(true);
                logoutButton.gameObject.SetActive(false);

            }
            else
            {
                loginButton.gameObject.SetActive(false);
                logoutButton.gameObject.SetActive(true);
            }
        }
    }
    // ---------------- DIFFICULTY SELECTION ----------------

    public void OnSelectEasy()
    {
        StartGame("easy");
    }

    public void OnSelectNormal()
    {
        StartGame("normal");
    }

    public void OnSelectHard()
    {
        StartGame("hard");
    }

    

    // ---------------- START GAME ----------------

    private void StartGame(string difficulty)
    {
        Debug.Log($"Starting game. SinglePlayer: {isSinglePlayer}, Difficulty: {difficulty}");


        GameSettingsManager.Instance.SetDifficulty(difficulty); 

        SceneManager.LoadScene("2.Ingame");
    }
}
