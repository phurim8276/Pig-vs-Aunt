using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpecialItemManager : MonoBehaviour
{
    public PlayerShoot playerShoot;
    public PlayerHealth playerHealth;

    public PlayerShoot playerShoot2;
    public PlayerHealth playerHealth2;

    [Header("UI Buttons")]
    public Button powerThrowButton;
    public Button doubleAttackButton;
    public Button healButton;

   

    [Header("UI Buttons - Player 2")]
    public Button powerThrowButton2;
    public Button doubleAttackButton2;
    public Button healButton2;


    [Header("Item Values")]
    public float healAmount = 50f; // Heal 50 HP
    public float powerThrowDamage = 10f; // Fixed damage for Power Throw
    public float doubleAttackDamage = 5f; // Fixed damage for Double Attack

    [Header("Text")]
    public TextMeshProUGUI auntItemText;
    public TextMeshProUGUI pigItemText;

    private void Start()
    {
        // Player 1 buttons
        powerThrowButton.onClick.AddListener(() => UseItem(SpecialItemType.PowerThrow, playerShoot, playerHealth, powerThrowButton));
        doubleAttackButton.onClick.AddListener(() => UseItem(SpecialItemType.DoubleAttack, playerShoot, playerHealth, doubleAttackButton));
        healButton.onClick.AddListener(() => UseItem(SpecialItemType.Heal, playerShoot, playerHealth, healButton));

        // Player 2 buttons
        powerThrowButton2.onClick.AddListener(() => UseItem(SpecialItemType.PowerThrow, playerShoot2, playerHealth2, powerThrowButton2));
        doubleAttackButton2.onClick.AddListener(() => UseItem(SpecialItemType.DoubleAttack, playerShoot2, playerHealth2, doubleAttackButton2));
        healButton2.onClick.AddListener(() => UseItem(SpecialItemType.Heal, playerShoot2, playerHealth2, healButton2));

        // Load database values (same as before)
        LoadItemValuesFromDatabase();

        auntItemText.gameObject.SetActive(false);
        pigItemText.gameObject.SetActive(false);
        
    }

    private void LoadItemValuesFromDatabase()
    {
        ItemDatabase db = SOHelper.LoadSOFromPath<ItemDatabase>("Data/ItemDatabase");

        if (float.TryParse(db.data.Heal.HP, out float tempHealAmount))
            healAmount = tempHealAmount;

        if (float.TryParse(db.data.PowerThrow.Damage, out float tempPowerThrowDamage))
            powerThrowDamage = tempPowerThrowDamage;

        if (float.TryParse(db.data.DoubleAttack.Damage, out float tempDoubleAttackDamage))
            doubleAttackDamage = tempDoubleAttackDamage;
    }


    public void UseItem(SpecialItemType itemType, PlayerShoot shoot, PlayerHealth health, Button button)
    {
        switch (itemType)
        {
            case SpecialItemType.PowerThrow:
                shoot.ActivatePowerThrow(powerThrowDamage);
                shoot.HasUsedPowerThrow = true;
                button.gameObject.SetActive(false);
                Debug.Log($"Power Throw Activated with {powerThrowDamage} damage for {shoot.name}!");

                // Show item text for Aunt or Pig
                if (shoot == playerShoot)
                {
                    ShowItemText(auntItemText, "Power Throw!");
                }
                else if (shoot == playerShoot2)
                {
                    ShowItemText(pigItemText, "Power Throw!");
                }
                break;

            case SpecialItemType.DoubleAttack:
                shoot.ActivateDoubleAttack(doubleAttackDamage);
                button.gameObject.SetActive(false);
                button.interactable = false;
                Debug.Log($"Double Attack Activated with {doubleAttackDamage} damage per shot for {shoot.name}!");

                if (shoot == playerShoot)
                {
                    ShowItemText(auntItemText, "Double Attack!");
                }
                else if (shoot == playerShoot2)
                {
                    ShowItemText(pigItemText, "Double Attack!");
                }
                break;

            case SpecialItemType.Heal:
                if (!health.HasUsedHeal)
                {
                    health.Heal(healAmount);
                    button.gameObject.SetActive(false);
                    Debug.Log($"Healed {healAmount} HP for {health.name}!");

                    if (shoot == playerShoot)
                    {
                        ShowItemText(auntItemText, "Heal!");
                    }
                    else if (shoot == playerShoot2)
                    {
                        ShowItemText(pigItemText, "Heal!");
                    }
                }
                else
                {
                    Debug.Log($"{health.name} already used Heal.");
                }
                break;
        }
    }
    public void BotShowText(string mode)
    {
        switch (mode)
        {
            case "hard":
                ShowItemText(pigItemText, "Power Throw!");
                break;
            case "normal":
                ShowItemText(pigItemText, "Double Attack!");
                break;
            default:
                Debug.LogWarning("Unknown mode for bot text display: " + mode);
                break;
        }

    }
    private Coroutine auntTextFadeCoroutine;
    private Coroutine pigTextFadeCoroutine;

    private void ShowItemText(TextMeshProUGUI textElement, string message)
    {
        if (textElement == null) return;

        // Stop any previous fade coroutine for this text
        if (textElement == auntItemText && auntTextFadeCoroutine != null)
        {
            StopCoroutine(auntTextFadeCoroutine);
        }
        else if (textElement == pigItemText && pigTextFadeCoroutine != null)
        {
            StopCoroutine(pigTextFadeCoroutine);
        }

        textElement.text = message;
        textElement.gameObject.SetActive(true);
        textElement.alpha = 1f;

        // Start fade out coroutine after showing text
        if (textElement == auntItemText)
        {
            auntTextFadeCoroutine = StartCoroutine(FadeOutText(textElement, 1f));
        }
        else if (textElement == pigItemText)
        {
            pigTextFadeCoroutine = StartCoroutine(FadeOutText(textElement, 1f));
        }
    }
    private IEnumerator FadeOutText(TextMeshProUGUI textElement, float delay)
    {
        // Wait before starting fade out
        yield return new WaitForSeconds(delay);

        float fadeDuration = 1f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            textElement.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        textElement.alpha = 0f;
        textElement.gameObject.SetActive(false);
    }


    public void DisableAllButtons()
    {
        powerThrowButton.interactable = false;
        doubleAttackButton.interactable = false;
        healButton.interactable = false;

        powerThrowButton2.interactable = false;
        doubleAttackButton2.interactable = false;
        healButton2.interactable = false;
    }

    public void DisablePigButton()
    {
        powerThrowButton2.gameObject.SetActive(false);
        doubleAttackButton2.gameObject.SetActive(false);
        healButton2.gameObject.SetActive(false);
    }
    public void EnableAvailableButtons(string player)
    {
        if(player == "pig")
        {
            if (!playerShoot2.HasUsedPowerThrow)
                powerThrowButton2.interactable = true;
            if (!playerShoot2.HasUsedDoubleAttack)
                doubleAttackButton2.interactable = true;
            if (!playerHealth2.HasUsedHeal)
                healButton2.interactable = true;
        }
        else if(player == "aunt")
        {
            if (!playerShoot.HasUsedPowerThrow)
                powerThrowButton.interactable = true;
            if (!playerShoot.HasUsedDoubleAttack)
                doubleAttackButton.interactable = true;
            if (!playerHealth.HasUsedHeal)
                healButton.interactable = true;
            
        }
        else
        {

            powerThrowButton.interactable = false;
            doubleAttackButton.interactable = false;
            healButton.interactable = false;

            powerThrowButton2.interactable = false;
            doubleAttackButton2.interactable = false;
            healButton2.interactable = false;
        }

    }
    public void ResetAllUsage()
    {
        playerShoot.HasUsedPowerThrow = false;
        playerShoot.HasUsedDoubleAttack = false;
        playerHealth.ResetHealUsage();

        playerShoot2.HasUsedPowerThrow = false;
        playerShoot2.HasUsedDoubleAttack = false;
        playerHealth2.ResetHealUsage();

        EnableAvailableButtons("");
    }



}
