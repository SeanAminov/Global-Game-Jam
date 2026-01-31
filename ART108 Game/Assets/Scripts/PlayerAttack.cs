using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int attackDamage = 1;
    public float attackRange = 1.5f;
    public Transform attackPoint;
    public LayerMask enemyLayer;

    [Header("Attack Cooldown")]
    public float attackCooldown = 0.5f;
    private float lastAttackTime = -999f;

    [Header("Knockback Settings")]
    public Rigidbody2D playerRb;
    public float playerKnockbackForce = 3f;
    public float enemyKnockbackForce = 5f;
    public float attackLockTime = 0.15f;
    
    private PlayerMovement playerMovement;
    private PlayerAnimator playerAnimator;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimator = GetComponent<PlayerAnimator>();
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed && Time.time >= lastAttackTime + attackCooldown)
        {
            Debug.Log("Attack performed");
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }

    private void PerformAttack()
    {
        // Trigger attack animation immediately (before anything else)
        if (playerAnimator != null)
        {
            playerAnimator.TriggerAttack();
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        Debug.Log("Enemies hit: " + hitEnemies.Length);

        foreach (Collider2D enemy in hitEnemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                // Calculate knockback direction (away from player)
                Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                enemyScript.TakeDamage(attackDamage, knockbackDirection, enemyKnockbackForce);
            }
        }

        // Apply knockback to player (slight recoil) - happens on every attack
        if (playerRb != null)
        {
            float direction = transform.localScale.x > 0 ? -1f : 1f;
            playerRb.linearVelocity = new Vector2(direction * playerKnockbackForce, playerRb.linearVelocity.y);
            
            if (playerMovement != null)
            {
                StartCoroutine(AttackLock());
            }
        }
    }

    private IEnumerator AttackLock()
    {
        playerMovement.isAttacking = true;
        yield return new WaitForSeconds(attackLockTime);
        playerMovement.isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
