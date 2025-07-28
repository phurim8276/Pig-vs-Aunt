using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 50f;
    public float currentHealth;
    public Image healthBar;

    public bool HasUsedHeal { get; private set; } = false;

    void Start()
    {
       
        currentHealth = maxHealth;
        Debug.Log($"{gameObject.name} starting health: {currentHealth}");
        UpdateUI();
    }
   

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateUI();

        if (currentHealth <= 0)
        {
            Debug.Log(gameObject.name + " is defeated!");
            // Trigger game over for this player
        }
    }
    public void Heal(float amount)
    {
        if (HasUsedHeal)
        {
            Debug.Log($"{gameObject.name} has already used Heal.");
            return; // Prevent healing if already used
        }

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Debug.Log($"{gameObject.name} healed by {amount}, current HP: {currentHealth}");
        UpdateUI();

        HasUsedHeal = true;  // Mark heal as used
    }

    public void ResetHealUsage()
    {
        HasUsedHeal = false;
    }

    void UpdateUI()
    {
        if (healthBar != null)
            healthBar.fillAmount = currentHealth / maxHealth;
    }
}
