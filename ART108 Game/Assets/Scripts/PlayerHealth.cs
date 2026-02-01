using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;

    private int currentHealth;

    public HealthUi healthUI;

    [Header("Knockback Settings")]
    public Rigidbody2D rb;
    public float knockbackForce = 10f;

    [Header("Visual")]
    public Color flashColor = Color.red;
    
    private SpriteRenderer spriteRenderer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        healthUI.SetMaxHearts(maxHealth);
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
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
        Debug.Log("Took damage");
        currentHealth -= damage;
        healthUI.UpdateHearts(currentHealth);

        // Flash effect
        StartCoroutine(FlashEffect());

        // Apply knockback
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(knockbackDirection.x * customKnockbackForce, customKnockbackForce * 0.5f);
        }

        if(currentHealth <= 0)
        {
            //dead
        }
    }

    private System.Collections.IEnumerator FlashEffect()
    {
        if (spriteRenderer == null)
        {
            yield break;
        }
        
        Color tempOriginal = spriteRenderer.color;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(0.25f);
        spriteRenderer.color = tempOriginal;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
