using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;

    private int currentHealth;

    public HealthUi healthUI;

    [Header("Knockback Settings")]
    public Rigidbody2D rb;
    public float knockbackForce = 10f;

    [Header("Invulnerability")]
    public float invulnerabilityDuration = 3f;
    private bool isInvulnerable = false;

    [Header("Visual")]
    public Color flashColor = Color.black;
    public float flashInterval = 0.25f;

    [Header("Death")]
    public GameObject deathUI;  // Assign death/restart panel
    
    [Header("Developer Options")]
    public bool godMode = false;  // Invincibility toggle
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDead = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        healthUI.SetMaxHearts(maxHealth);
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            // Ensure alpha is 1
            originalColor.a = 1f;
        }
    }

    // Force reset visibility if something goes wrong
    void LateUpdate()
    {
        // Safety check - if not invulnerable, ensure fully visible
        if (!isInvulnerable && spriteRenderer != null && spriteRenderer.color.a < 1f)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isInvulnerable)
            return;

        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if(enemy)
        {
            // Calculate knockback direction (away from enemy)
            Vector2 knockbackDirection = (transform.position - enemy.transform.position).normalized;
            ApplyDamage(enemy.damage, knockbackDirection, knockbackForce);
        }
    }

    public void ApplyDamage(int damage, Vector2 knockbackDirection, float customKnockbackForce)
    {   
        if (isInvulnerable || isDead || godMode)
            return;

        Debug.Log("Took damage");
        currentHealth -= damage;
        healthUI.UpdateHearts(currentHealth);

        // Flash red on damage
        if (spriteRenderer != null)
        {
            StartCoroutine(DamageFlash());
        }

        // Apply knockback
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(knockbackDirection.x * customKnockbackForce, customKnockbackForce * 0.5f);
        }

        // Start invulnerability
        StartCoroutine(InvulnerabilityCoroutine());

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        Debug.Log("Player died!");

        // Disable player controls
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null)
            attack.enabled = false;

        // Stop player movement
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // Show death UI
        if (deathUI != null)
        {
            deathUI.SetActive(true);
            Time.timeScale = 0f;  // Pause game
        }
    }

    private System.Collections.IEnumerator DamageFlash()
    {
        // Flash red briefly
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = originalColor;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        healthUI.UpdateHearts(currentHealth);
        Debug.Log($"Healed! Current health: {currentHealth}/{maxHealth}");
    }

    public void GainMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth = maxHealth;  // Fill to new max
        healthUI.SetMaxHearts(maxHealth);
        healthUI.UpdateHearts(currentHealth);
        Debug.Log($"Gained max health! Now at {currentHealth}/{maxHealth}");
    }

    private System.Collections.IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        // Just wait - no visual changes for now (debugging)
        yield return new WaitForSeconds(invulnerabilityDuration);

        isInvulnerable = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;  // Resume time
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScreen");
    }
}
