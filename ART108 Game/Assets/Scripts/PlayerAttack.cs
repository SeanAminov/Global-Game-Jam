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
    public float attackVisibleDuration = 0.3f;
    public float attackMoveDistance = 0.5f;

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
    private SpriteRenderer attackPointSprite;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimator = GetComponent<PlayerAnimator>();
        
        if (attackPoint != null)
        {
            attackPointSprite = attackPoint.GetComponent<SpriteRenderer>();
            if (attackPointSprite != null)
            {
                attackPointSprite.enabled = false; // Start invisible
            }
        }
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
        // Show attack point sprite
        if (attackPointSprite != null)
        {
            StartCoroutine(ShowAttackSprite());
        }

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

    private IEnumerator ShowAttackSprite()
    {
        if (attackPointSprite == null || attackPoint == null)
        {
            yield break;
        }

        attackPointSprite.enabled = true;

        Vector3 originalPos = attackPoint.localPosition;
        float direction = transform.localScale.x > 0 ? 1f : -1f;
        Vector3 extendedPos = originalPos + Vector3.right * attackMoveDistance * direction;

        // Move forward during attack
        float elapsed = 0f;
        while (elapsed < attackVisibleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / attackVisibleDuration;
            attackPoint.localPosition = Vector3.Lerp(originalPos, extendedPos, t * 0.6f); // 60% forward
            yield return null;
        }

        // Move back to original position
        elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.1f;
            attackPoint.localPosition = Vector3.Lerp(extendedPos, originalPos, t);
            yield return null;
        }

        attackPoint.localPosition = originalPos;
        attackPointSprite.enabled = false;
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
