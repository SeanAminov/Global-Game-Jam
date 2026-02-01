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
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        healthUI.SetMaxHearts(maxHealth);
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
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
        if (isInvulnerable)
            return;

        Debug.Log("Took damage");
        currentHealth -= damage;
        healthUI.UpdateHearts(currentHealth);

        // Apply knockback
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(knockbackDirection.x * customKnockbackForce, customKnockbackForce * 0.5f);
        }

        // Start invulnerability
        StartCoroutine(InvulnerabilityCoroutine());

        if(currentHealth <= 0)
        {
            //dead
        }
    }

    private System.Collections.IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        float elapsed = 0f;
        bool showFlash = true;

        while (elapsed < invulnerabilityDuration)
        {
            // Toggle between flash color and original
            if (spriteRenderer != null)
            {
                spriteRenderer.color = showFlash ? flashColor : originalColor;
            }
            showFlash = !showFlash;

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        // Reset to original color and end invulnerability
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        isInvulnerable = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
