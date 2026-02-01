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
    public TMPro.TextMeshProUGUI deathReasonText;  // Optional text to show death reason
    public Animator playerAnimator;  // For death animation
    public string deathAnimationTrigger = "Die";  // Animation trigger name
    
    [Header("Developer Options")]
    public bool godMode = false;  // Invincibility toggle
    
    [Header("Invulnerability Flashing")]
    public int flashCount = 6;  // Number of flash cycles during invulnerability
    public float minAlpha = 0.3f;  // Lowest opacity during flash
    
    [Header("Audio")]
    public AudioClip damageSound;
    private AudioSource audioSource;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDead = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        healthUI.SetMaxHearts(maxHealth);
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
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

    void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current == null)
            return;

        // Toggle god mode with I key
        if (UnityEngine.InputSystem.Keyboard.current[UnityEngine.InputSystem.Key.I].wasPressedThisFrame)
        {
            godMode = !godMode;
        }

        // Reload scene with R key
        if (UnityEngine.InputSystem.Keyboard.current[UnityEngine.InputSystem.Key.R].wasPressedThisFrame)
        {
            RestartGame();
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

        currentHealth -= damage;
        healthUI.UpdateHearts(currentHealth);

        // Play damage sound
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

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
            Die("You ran out of health!");
        }
    }

    public void Die(string reason = "You died!")
    {
        if (isDead)
            return;

        isDead = true;

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

        // Play death animation
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(deathAnimationTrigger);
        }

        // Show death UI after short delay for animation
        StartCoroutine(ShowDeathUIAfterDelay(reason, 1f));
    }

    private System.Collections.IEnumerator ShowDeathUIAfterDelay(string reason, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (deathUI != null)
        {
            deathUI.SetActive(true);
            
            if (deathReasonText != null)
            {
                deathReasonText.text = reason;
            }
            
            Time.timeScale = 0f;
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
    }

    public void GainMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;  // Heal by the same amount, not fill to max
        currentHealth = Mathf.Min(currentHealth, maxHealth);  // Clamp to max
        healthUI.SetMaxHearts(maxHealth);
        healthUI.UpdateHearts(currentHealth);
    }

    private System.Collections.IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        if (spriteRenderer != null && flashCount > 0)
        {
            float flashDuration = invulnerabilityDuration / flashCount;
            float halfFlash = flashDuration / 2f;

            for (int i = 0; i < flashCount; i++)
            {
                // Fade out
                float elapsed = 0f;
                while (elapsed < halfFlash)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / halfFlash;
                    Color c = spriteRenderer.color;
                    c.a = Mathf.Lerp(1f, minAlpha, t);
                    spriteRenderer.color = c;
                    yield return null;
                }

                // Fade in
                elapsed = 0f;
                while (elapsed < halfFlash)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / halfFlash;
                    Color c = spriteRenderer.color;
                    c.a = Mathf.Lerp(minAlpha, 1f, t);
                    spriteRenderer.color = c;
                    yield return null;
                }
            }

            // Ensure fully visible at end
            Color final = spriteRenderer.color;
            final.a = 1f;
            spriteRenderer.color = final;
        }
        else
        {
            yield return new WaitForSeconds(invulnerabilityDuration);
        }

        isInvulnerable = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;  // Resume time
        CheckpointManager.Instance?.ResetToStart();  // Clear checkpoint
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    public void RestartFromCheckpoint()
    {
        Time.timeScale = 1f;  // Resume time
        
        // Get respawn position
        Vector3 respawnPos = CheckpointManager.Instance?.GetRespawnPosition() ?? transform.position;
        
        // Reset player state
        isDead = false;
        currentHealth = maxHealth;
        healthUI.UpdateHearts(currentHealth);
        
        // Re-enable controls
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = true;

        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null)
            attack.enabled = true;
        
        // Move player to checkpoint
        transform.position = respawnPos;
        
        // Hide death UI
        if (deathUI != null)
        {
            deathUI.SetActive(false);
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScreen");
    }
}
