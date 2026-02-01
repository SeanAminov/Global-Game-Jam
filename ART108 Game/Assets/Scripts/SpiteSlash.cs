using UnityEngine;
using System.Collections.Generic;

public class SpiteSlash : MonoBehaviour
{
    [HideInInspector] public int damage = 3;
    [HideInInspector] public float knockbackForce = 8f;
    [HideInInspector] public bool trackHitEnemies = false;

    public LayerMask enemyLayer;

    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

    private void Start()
    {
        // Auto-detect enemy layer if not set
        if (enemyLayer == 0)
        {
            enemyLayer = LayerMask.GetMask("Enemy");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if hit an enemy
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            // Skip if we've already hit this enemy (for pulse attack)
            if (trackHitEnemies && hitEnemies.Contains(collision.gameObject))
                return;

            // Mark as hit
            if (trackHitEnemies)
                hitEnemies.Add(collision.gameObject);

            // Apply damage
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Calculate knockback direction (away from player/center)
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                
                // Ensure knockback has upward component like normal attacks
                if (knockbackDir.y < 0.3f)
                    knockbackDir.y = 0.3f;
                knockbackDir = knockbackDir.normalized;
                
                enemy.TakeDamage(damage, knockbackDir, knockbackForce);
                Debug.Log($"SpiteSlash hit {collision.name}: damage={damage}, knockback={knockbackForce}, dir={knockbackDir}");
            }

            // Only destroy on hit if NOT tracking (for regular projectiles)
            if (!trackHitEnemies)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // For expanding hitbox - also check during stay
        OnTriggerEnter2D(collision);
    }
}
