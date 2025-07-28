using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    [Header("Players")]
    public PlayerShoot pig;
    public PlayerShoot aunt;

    public PlayerHealth pigHealth;
    public PlayerHealth auntHealth;

    private PlayerShoot currentShooter;

    [Header("UI Arrows")]
    public Image pigArrow;
    public Image auntArrow;

    [Header("Wind")]
    private WindManager windManager;

    private bool gameOver = false;
    private bool isMockeryPlaying = false;
    private bool isHitAnimationPlaying = false;

    private Coroutine pigArrowBlinkCoroutine;
    private Coroutine auntArrowBlinkCoroutine;

    [Header("Special Items")]
    public SpecialItemManager specialItemManager;

    [Header("Game Timer UI")]
    [SerializeField]
    private float gameTimer = 0f;  // Tracks elapsed time

    [Header("Game Over UI")]
    public GameObject gameOverPanel;  // Assign your GameOverPanel in Canvas
    public TextMeshProUGUI finalTimeText;        // Text inside GameOverPanel to show final time

    public TextMeshProUGUI endGameText;
    private void Awake()
    {
        windManager = FindAnyObjectByType<WindManager>();

        // Set up player healths
        if (GameSettingsManager.Instance.isSinglePlayer)
        {
            ItemDatabase db = SOHelper.LoadSOFromPath<ItemDatabase>("Data/ItemDatabase");
            float tempHealth, tempEnemyHP;
            if (GameSettingsManager.Instance.difficulty == "easy")
            {
                if (float.TryParse(db.data.EnemyHP_Easy.HP, out tempEnemyHP))
                    pigHealth.maxHealth = tempEnemyHP;
            }
            else if (GameSettingsManager.Instance.difficulty == "normal")
            {
                if (float.TryParse(db.data.EnemyHP_Normal.HP, out tempEnemyHP))
                    pigHealth.maxHealth = tempEnemyHP;
            }
            else if (GameSettingsManager.Instance.difficulty == "hard")
            {
                if (float.TryParse(db.data.EnemyHP_Hard.HP, out tempEnemyHP))
                    pigHealth.maxHealth = tempEnemyHP;
            }
            if (float.TryParse(db.data.PlayerHP.HP, out tempHealth))
            {
                auntHealth.maxHealth = tempHealth;
            }
        }
        else
        {
            ItemDatabase db = SOHelper.LoadSOFromPath<ItemDatabase>("Data/ItemDatabase");
            float tempHealth;
            if (float.TryParse(db.data.PlayerHP.HP, out tempHealth))
            {
                auntHealth.maxHealth = tempHealth;
                pigHealth.maxHealth = tempHealth;
            }
        }
    }

    private void OnEnable()
    {
        Projectile.OnAnyProjectileLanded += OnProjectileLanded;
    }

    private void OnDisable()
    {
        Projectile.OnAnyProjectileLanded -= OnProjectileLanded;
    }

    void Start()
    {
        if (GameSettingsManager.Instance.isSinglePlayer)
        {
            pig.isBot = true;
        }
        else
        {
            pig.isBot = false;
        }

        if (pig.isBot)
        {
            specialItemManager.DisablePigButton();
        }

        currentShooter = aunt;
        currentShooter.StartTurnTimer();

        pig.EnableShooting(false);
        aunt.EnableShooting(true);

        StartArrowBlink();

        Debug.Log("Game Mode: " + (GameSettingsManager.Instance.isSinglePlayer ? "1 Player" : "2 Player"));
        Debug.Log("Difficulty: " + GameSettingsManager.Instance.difficulty);

        specialItemManager.DisableAllButtons();
        if (currentShooter == pig)
        {
            specialItemManager.EnableAvailableButtons("pig");
        }
        else
        {
            specialItemManager.EnableAvailableButtons("aunt");
        }

        gameTimer = 0f;


        // Hide GameOverPanel at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    // ---------------- UPDATE ----------------
    void Update()
    {
        if (!gameOver)
        {
            gameTimer += Time.deltaTime;
            UpdateGameTimerDisplay(gameTimer);
        }
    }

    private void UpdateGameTimerDisplay(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

    }

    // ---------------- TURN MANAGEMENT ----------------
    private void OnProjectileLanded()
    {
        if (gameOver || isMockeryPlaying || isHitAnimationPlaying) return;
        EndTurn();
    }

    public void EndTurn()
    {
        if (gameOver) return;

        specialItemManager.DisableAllButtons();

        CheckWinCondition();
        if (gameOver) return;

        currentShooter.EnableShooting(false);
        currentShooter.EndTurnTimer();

        currentShooter = (currentShooter == pig) ? aunt : pig;

        currentShooter.EnableShooting(true);
        currentShooter.StartTurnTimer();

        StartArrowBlink();
        windManager.ChangeWind();

        Debug.Log($"{currentShooter.name}'s turn!");

        if (currentShooter == pig)
        {
            specialItemManager.EnableAvailableButtons("pig");
        }
        else
        {
            specialItemManager.EnableAvailableButtons("aunt");
        }
    }

    public void CheckWinCondition()
    {
        if (pigHealth.currentHealth <= 0)
        {
            gameOver = true;
            Debug.Log("Aunt Wins!");
            EndGame();
        }
        else if (auntHealth.currentHealth <= 0)
        {
            gameOver = true;
            Debug.Log("Pig Wins!");
            EndGame();
        }
    }

    private void EndGame()
    {
        pig.EnableShooting(false);
        aunt.EnableShooting(false);
        HideArrows();

        Debug.Log($"Game Over! Total Time: {gameTimer:0.00} seconds");

        StartCoroutine(ShowGameOverPanel());

        // Play Win/Lose animations
        if (pigHealth.currentHealth <= 0)
        {
            aunt.GetComponent<CharacterAnimation>()?.PlayWin();
            pig.GetComponent<CharacterAnimation>()?.PlayLose();
        }
        else
        {
            pig.GetComponent<CharacterAnimation>()?.PlayWin();
            aunt.GetComponent<CharacterAnimation>()?.PlayLose();
        }
    }
    private IEnumerator ShowGameOverPanel()
    {
        yield return new WaitForSeconds(1.5f); // short delay
        endGameText.text = pigHealth.currentHealth <= 0 ? "Aunt Wins!" : "Pig Wins!";
        gameOverPanel.SetActive(true);

        if (finalTimeText != null)
        {
            int minutes = Mathf.FloorToInt(gameTimer / 60f);
            int seconds = Mathf.FloorToInt(gameTimer % 60f);
            finalTimeText.text = $"Total Time: {minutes:00}:{seconds:00}";
        }
    }
    // ---------------- ARROW BLINKING ----------------
    void StartArrowBlink()
    {
        HideArrows();

        if (currentShooter == pig)
            pigArrowBlinkCoroutine = StartCoroutine(BlinkArrow(pigArrow));
        else
            auntArrowBlinkCoroutine = StartCoroutine(BlinkArrow(auntArrow));
    }

    public void HideArrows()
    {
        if (pigArrowBlinkCoroutine != null) StopCoroutine(pigArrowBlinkCoroutine);
        if (auntArrowBlinkCoroutine != null) StopCoroutine(auntArrowBlinkCoroutine);

        pigArrow.color = new Color(pigArrow.color.r, pigArrow.color.g, pigArrow.color.b, 0f);
        auntArrow.color = new Color(auntArrow.color.r, auntArrow.color.g, auntArrow.color.b, 0f);

        pigArrowBlinkCoroutine = null;
        auntArrowBlinkCoroutine = null;
    }

    IEnumerator BlinkArrow(Image arrow)
    {
        while (true)
        {
            for (float t = 0f; t <= 1f; t += Time.deltaTime * 3)
            {
                arrow.color = new Color(arrow.color.r, arrow.color.g, arrow.color.b, t);
                yield return null;
            }
            for (float t = 1f; t >= 0f; t -= Time.deltaTime * 3)
            {
                arrow.color = new Color(arrow.color.r, arrow.color.g, arrow.color.b, t);
                yield return null;
            }
        }
    }

    public PlayerShoot GetCurrentShooter() => currentShooter;

    public void TriggerMockeryBeforeTurn(CharacterAnimation mocker)
    {
        if (gameOver || isMockeryPlaying) return;

        pig.EnableShooting(false);
        aunt.EnableShooting(false);

        isMockeryPlaying = true;
        mocker.PlayMockery(() =>
        {
            isMockeryPlaying = false;
            EndTurn();
        });
    }

    public void TriggerHitBeforeTurn(CharacterAnimation hitPlayer)
    {
        if (gameOver || isHitAnimationPlaying) return;

        pig.EnableShooting(false);
        aunt.EnableShooting(false);

        isHitAnimationPlaying = true;
        hitPlayer.PlayHit(() =>
        {
            isHitAnimationPlaying = false;
            EndTurn();
        });
    }
    public void ReplayGame()
    {
        Debug.Log("Replay Game!");
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void GoToMainMenu()
    {
        Debug.Log("Go to Main Menu!");
        SceneManager.LoadScene("1.Menu");
    }
}
