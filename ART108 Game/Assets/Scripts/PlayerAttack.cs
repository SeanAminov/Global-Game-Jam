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

    [Header("Attack Sprite Animation")]
    public float attackVisibleDuration = 0.3f;
    public float attackMoveDistance = 0.5f;

    [Header("Cooldown")]
    public float attackCooldown = 0.5f;

    [Header("Knockback")]
    public Rigidbody2D playerRb;
    public float playerKnockbackForce = 3f;
    public float enemyKnockbackForce = 5f;
    public float attackLockTime = 0.15f;

    private PlayerMovement playerMovement;
    private PlayerAnimator playerAnimator;
    private SpriteRenderer attackPointSprite;
    private float lastAttackTime = -999f;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimator = GetComponent<PlayerAnimator>();

        if (attackPoint != null)
        {
            attackPointSprite = attackPoint.GetComponent<SpriteRenderer>();
            if (attackPointSprite != null)
            {
                attackPointSprite.enabled = false;
            }
        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed && Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }

    private void PerformAttack()
    {
        // Capture facing direction at attack time
        bool facingRight = playerMovement != null ? playerMovement.IsFacingRight : transform.localScale.x > 0;

        // Show attack sprite
        if (attackPointSprite != null)
        {
            StartCoroutine(ShowAttackSprite(facingRight));
        }

        // Trigger animation
        if (playerAnimator != null)
        {
            playerAnimator.TriggerAttack();
        }

        // Damage enemies
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                enemyScript.TakeDamage(attackDamage, knockbackDir, enemyKnockbackForce);
            }
        }

        // Player recoil
        if (playerRb != null)
        {
            float recoilDir = facingRight ? -1f : 1f;
            playerRb.linearVelocity = new Vector2(recoilDir * playerKnockbackForce, playerRb.linearVelocity.y);

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

    private IEnumerator ShowAttackSprite(bool facingRight)
    {
        if (attackPointSprite == null || attackPoint == null)
            yield break;

        // Store original state
        Transform originalParent = attackPoint.parent;
        Vector3 originalLocalPos = attackPoint.localPosition;
        Vector3 originalLocalScale = attackPoint.localScale;

        // Direction: right = +1, left = -1
        float dir = facingRight ? 1f : -1f;
        Vector3 startPos = attackPoint.position;
        Vector3 endPos = startPos + Vector3.right * attackMoveDistance * dir;

        // Detach from player so it doesn't follow
        attackPoint.SetParent(null, true);
        attackPoint.localScale = Vector3.one; // Always positive scale, never flipped
        attackPointSprite.flipX = facingRight; // Flip when facing right (sprite drawn facing left by default)
        attackPointSprite.enabled = true;

        // Animate movement
        float elapsed = 0f;
        while (elapsed < attackVisibleDuration)
        {
            elapsed += Time.deltaTime;
            attackPoint.position = Vector3.Lerp(startPos, endPos, elapsed / attackVisibleDuration);
            yield return null;
        }

        // Restore
        attackPointSprite.enabled = false;
        attackPoint.SetParent(originalParent, true);
        attackPoint.localPosition = originalLocalPos;
        attackPoint.localScale = originalLocalScale;
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
