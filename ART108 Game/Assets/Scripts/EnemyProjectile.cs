using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 8f;
    public float lifetime = 3f;
    public int damage = 1;
    public float knockbackForce = 5f;
    
    [Header("Collision")]
    public LayerMask groundLayers;

    private Vector2 direction = Vector2.right;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool initialized = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 newDirection, float newSpeed, float newLifetime, int newDamage, float newKnockbackForce)
    {
        direction = newDirection.normalized;
        speed = newSpeed;
        lifetime = newLifetime;
        damage = newDamage;
        knockbackForce = newKnockbackForce;
        initialized = true;

        // Set velocity
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        // Rotate to face direction of travel
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject other)
    {
        // Check ground/wall layers
        int hitLayerMask = 1 << other.layer;
        if ((hitLayerMask & groundLayers.value) != 0)
        {
            Destroy(gameObject);
            return;
        }

        // Check player hit
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.ApplyDamage(damage, direction, knockbackForce);
            }
            Destroy(gameObject);
            return;
        }

        // Ignore hitting the enemy that spawned it (check by layer instead)
        if (other.layer == LayerMask.NameToLayer("Enemy"))
        {
            return;
        }
    }
}
