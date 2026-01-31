using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;

    private int currentHealth;

    public HealthUi healthUI;

    [Header("Knockback Settings")]
    public Rigidbody2D rb;
    public float knockbackForce = 10f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        healthUI.SetMaxHearts(maxHealth);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if(enemy)
        {
            // Calculate knockback direction (away from enemy)
            Vector2 knockbackDirection = (transform.position - enemy.transform.position).normalized;
            TakeDamage(enemy.damage, knockbackDirection);
        }
    }

    private void TakeDamage(int damage, Vector2 knockbackDirection)
    {   
        Debug.Log("Took damage");
        currentHealth -= damage;
        healthUI.UpdateHearts(currentHealth);

        // Apply knockback
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(knockbackDirection.x * knockbackForce, knockbackForce * 0.5f);
        }

        if(currentHealth <= 0)
        {
            //dead
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
