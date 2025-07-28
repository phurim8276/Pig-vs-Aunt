using UnityEngine;
using System;
using System.Collections;  // For Action delegate

public class Projectile : MonoBehaviour
{
    public static Action OnAnyProjectileLanded;

    public enum AttackType { Normal, Small }
    public AttackType attackType = AttackType.Normal;

    [Header("Damage Settings")]
    public float normalDamage = 10f;
    public float smallDamage = 5f;

    private Rigidbody2D rb;
    private bool isLaunched = false;
    private bool hasHit = false; // Tracks if this projectile hit a player

    void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();
    } 
    
   

    public void Launch(Vector2 direction, float power)
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(direction * power, ForceMode2D.Impulse);
        isLaunched = true;
    }

    void FixedUpdate()
    {
        if (isLaunched)
            rb.AddForce(new Vector2(WindManager.windForce, 0f) , ForceMode2D.Force);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isLaunched) return;

        PlayerShoot thrower = FindThrower();
        if (thrower != null && collision.gameObject == thrower.gameObject)
        {
            StartCoroutine(DelayedLand());
            return;
        }

        if (collision.gameObject.CompareTag("Aunt") || collision.gameObject.CompareTag("Pig"))
        {
            hasHit = true;

            ContactPoint2D contact = collision.contacts[0]; // Get the first contact point

            // Check if hit on head
            bool hitHead = IsHitOnHead(collision.gameObject, contact.point);

            attackType = hitHead ? AttackType.Normal : AttackType.Small;

            DealDamageAndAnimate(collision.transform);
        }

        StartCoroutine(DelayedLand());
    }
    IEnumerator DelayedLand()
    {
        yield return new WaitForSeconds(0.2f); // wait a bit to ensure all collisions are processed
        LandProjectile();
    }
    // Option 1: Check if collision point is inside head collider
    bool IsHitOnHead(GameObject target, Vector2 hitPoint)
    {
        // Try to find a collider named "Head" on the target or its children
        Collider2D headCollider = null;

        Collider2D[] colliders = target.GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            if (col.gameObject.name.ToLower().Contains("Head"))
            {
                headCollider = col;
                break;
            }
        }

        if (headCollider != null)
        {
            return headCollider.OverlapPoint(hitPoint);
        }
        else
        {

            Collider2D mainCollider = target.GetComponent<Collider2D>();
            if (mainCollider == null) return false;

            Bounds bounds = mainCollider.bounds;
            float headThreshold = bounds.min.y + bounds.size.y * 0.75f; // top 25% as head

            return hitPoint.y >= headThreshold;
        }
    }
    void DealDamageAndAnimate(Transform hitObject)
    {
        float damage = (attackType == AttackType.Normal) ? normalDamage : smallDamage;

        PlayerHealth targetHealth = hitObject.GetComponentInParent<PlayerHealth>();
        targetHealth?.TakeDamage(damage);
        Debug.Log($"Projectile hit {hitObject.name} with {attackType} damage: {damage}");
        // Play hit animation via TurnManager
        TurnManager tm = FindObjectOfType<TurnManager>();
        CharacterAnimation anim = hitObject.GetComponent<CharacterAnimation>();

        if (tm != null && anim != null)
        {
            tm.TriggerHitBeforeTurn(anim);
        }
        else
        {
            // If no TurnManager, just play animation directly
            anim?.PlayHit();
            OnAnyProjectileLanded?.Invoke();
        }
    }

    void LandProjectile()
    {
        TurnManager tm = FindObjectOfType<TurnManager>();

        // If projectile misses, trigger mockery animation
        if (!hasHit && tm != null)
        {
            PlayerShoot otherPlayer = (tm.GetCurrentShooter() == tm.pig) ? tm.aunt : tm.pig;
            CharacterAnimation mockerAnim = otherPlayer.GetComponent<CharacterAnimation>();

            if (mockerAnim != null)
            {
                tm.TriggerMockeryBeforeTurn(mockerAnim);
            }
        }

        // ALWAYS invoke the event (hit or miss)
        OnAnyProjectileLanded?.Invoke();
        Destroy(gameObject, 0.03f);
    }



    PlayerShoot FindThrower()
    {
        TurnManager tm = FindObjectOfType<TurnManager>();
        return tm?.GetCurrentShooter();
    }
}
