using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("Trap Settings")]
    public float knockbackForce = 10f;
    public int damageAmount = 1;
    public bool causesDamage = true;

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerHit(collision.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerHit(collision.gameObject);
        }
    }

    private void HandlePlayerHit(GameObject player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // Reset vertical velocity and apply upward knockback
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * knockbackForce, ForceMode2D.Impulse);
        }

        // Apply damage if enabled
        if (causesDamage && playerHealth != null && damageAmount > 0)
        {
            playerHealth.ApplyDamage(damageAmount, Vector2.up, knockbackForce);
        }
    }
}
