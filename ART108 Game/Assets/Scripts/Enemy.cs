using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("References")]
    public Transform Player;
    public Transform attackPoint;
    public LayerMask groundLayer;
    public LayerMask playerLayer;

    [Header("Movement")]
    public float chaseSpeed = 2f;
    public float jumpForce = 2f;
    public float detectionRange = 10f;

    [Header("Attack")]
    public float attackRange = 1.2f;
    public float attackRangeBuffer = 0.75f;
    public float attackCooldown = 1f;
    public float attackKnockbackForce = 8f;
    public int damage = 1;

    [Header("Attack Sprite Animation")]
    public float attackVisibleDuration = 0.3f;
    public float attackMoveDistance = 0.5f;

    [Header("Random Attack")]
    public float minRandomAttackInterval = 1.0f;
    public float maxRandomAttackInterval = 2.5f;

    [Header("Self Knockback on Attack")]
    public float selfKnockbackForce = 2f;
    public float selfKnockbackUpward = 0.5f;

    [Header("Health")]
    public int maxHealth = 3;

    [Header("Visual")]
    public Color flashColor = Color.red;

    [Header("Debug")]
    public bool debugAttackLogs = false;

    // Private state
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer attackPointSprite;
    private Color originalColor;
    private int currentHealth;
    private float lastAttackTime;
    private float nextRandomAttackTime;
    private bool isGrounded;
    private bool shouldJump;
    private Coroutine flashCoroutine;
    private float attackKnockbackEndTime;

    // Animator parameters
    private const string IS_WALKING = "isWalking";
    private const string ATTACK_TRIGGER = "attack";

    // Facing: use transform.localScale.x to determine direction
    // NOTE: If your sprite faces LEFT by default, positive scale = facing left
    private bool IsFacingRight => transform.localScale.x < 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (attackPoint != null)
        {
            attackPointSprite = attackPoint.GetComponent<SpriteRenderer>();
            if (attackPointSprite != null)
                attackPointSprite.enabled = false;
        }

        currentHealth = maxHealth;
        ScheduleNextRandomAttack();
    }

    void Update()
    {
        if (Player == null)
        {
            if (debugAttackLogs) Debug.Log($"{name}: Player is NULL!");
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, Player.position);

        if (debugAttackLogs)
            Debug.Log($"{name}: Distance={distanceToPlayer:F2}, DetectionRange={detectionRange}, AttackRange={attackRange}");

        if (distanceToPlayer > detectionRange)
            return;

        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 2f, groundLayer);
        float directionToPlayer = Mathf.Sign(Player.position.x - transform.position.x);

        // Check attack range from attack point
        Vector2 attackOrigin = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        float distanceFromAttackPoint = Vector2.Distance(attackOrigin, Player.position);
        bool isPlayerInAttackRange = distanceFromAttackPoint <= (attackRange + attackRangeBuffer);

        // Attack if in range
        if (isPlayerInAttackRange)
        {
            if (debugAttackLogs) Debug.Log($"{name}: Player IN ATTACK RANGE!");
            TryAttack();
        }

        // Random attack timer
        if (Time.time >= nextRandomAttackTime)
        {
            ScheduleNextRandomAttack();
            if (debugAttackLogs) Debug.Log($"{name}: Random attack attempt");
            TryAttack();
        }

        // Face player
        if ((directionToPlayer > 0 && !IsFacingRight) || (directionToPlayer < 0 && IsFacingRight))
            Flip();

        // Chase player if grounded and not in attack range and not in knockback
        if (isGrounded && !isPlayerInAttackRange && Time.time > attackKnockbackEndTime)
        {
            rb.linearVelocity = new Vector2(directionToPlayer * chaseSpeed, rb.linearVelocity.y);
            CheckJump(directionToPlayer);
        }

        // Update animator
        if (animator != null)
            animator.SetBool(IS_WALKING, Mathf.Abs(rb.linearVelocity.x) > 0.1f && isGrounded);
    }

    void FixedUpdate()
    {
        if (isGrounded && shouldJump)
        {
            shouldJump = false;
            Vector2 dir = (Player.position - transform.position).normalized;
            rb.AddForce(new Vector2(dir.x * jumpForce, jumpForce), ForceMode2D.Impulse);
        }
    }

    private void CheckJump(float direction)
    {
        bool isPlayerAbove = Physics2D.Raycast(transform.position, Vector2.up, 1.5f, 1 << Player.gameObject.layer);
        RaycastHit2D groundInFront = Physics2D.Raycast(transform.position, new Vector2(direction, 0), 2f, groundLayer);
        RaycastHit2D gapAhead = Physics2D.Raycast(transform.position + new Vector3(direction, 0, 0), Vector2.down, 2f, groundLayer);
        RaycastHit2D platformAbove = Physics2D.Raycast(transform.position, Vector2.up, 3f, groundLayer);

        if ((!groundInFront.collider && !gapAhead.collider) || (isPlayerAbove && platformAbove.collider))
            shouldJump = true;
    }

    private void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;

        if (debugAttackLogs)
            Debug.Log($"{name}: TryAttack executing! IsFacingRight={IsFacingRight}");

        // Trigger animation
        if (animator != null)
            animator.SetTrigger(ATTACK_TRIGGER);

        // Show attack sprite
        if (attackPointSprite != null)
            StartCoroutine(ShowAttackSprite());

        // Self knockback (push back when attacking) - always happens regardless of hit
        if (rb != null)
        {
            float knockbackDir = IsFacingRight ? -1f : 1f;
            Vector2 knockbackVelocity = new Vector2(knockbackDir * selfKnockbackForce, selfKnockbackUpward);
            rb.linearVelocity = knockbackVelocity;
            attackKnockbackEndTime = Time.time + 0.2f; // Prevent chase from overriding knockback
            
            if (debugAttackLogs)
                Debug.Log($"{name}: Self knockback applied! Direction={knockbackDir}, Force={selfKnockbackForce}, Velocity={knockbackVelocity}");
        }
        else if (debugAttackLogs)
        {
            Debug.Log($"{name}: rb is NULL - cannot apply self knockback!");
        }

        // Deal damage to player
        Vector2 origin = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        Collider2D playerHit = Physics2D.OverlapCircle(origin, attackRange, playerLayer);
        if (playerHit != null)
        {
            PlayerHealth playerHealth = playerHit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Vector2 knockbackDir = (playerHit.transform.position - transform.position).normalized;
                playerHealth.ApplyDamage(damage, knockbackDir, attackKnockbackForce);
            }
        }
    }

    private void ScheduleNextRandomAttack()
    {
        nextRandomAttackTime = Time.time + Random.Range(minRandomAttackInterval, maxRandomAttackInterval);
    }

    public void TakeDamage(int dmg, Vector2 knockbackDirection, float knockbackForce)
    {
        currentHealth -= dmg;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashEffect());

        rb.linearVelocity = new Vector2(knockbackDirection.x * knockbackForce, knockbackForce * 0.5f);

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator FlashEffect()
    {
        if (spriteRenderer == null)
            yield break;

        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(0.25f);
        spriteRenderer.color = originalColor;
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private IEnumerator ShowAttackSprite()
    {
        if (attackPointSprite == null || attackPoint == null)
            yield break;

        // Store original state
        Transform originalParent = attackPoint.parent;
        Vector3 originalLocalPos = attackPoint.localPosition;

        // Direction: right = +1, left = -1
        float dir = IsFacingRight ? 1f : -1f;
        Vector3 startPos = attackPoint.position;
        Vector3 endPos = startPos + Vector3.right * attackMoveDistance * dir;

        // Detach from bear so it doesn't follow
        attackPoint.SetParent(null, true);
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
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 origin = attackPoint != null ? attackPoint.position : transform.position;
        Gizmos.DrawWireSphere(origin, attackRange);
    }
}
