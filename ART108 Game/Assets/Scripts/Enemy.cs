using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    public Transform Player;
    public float chaseSpeed = 2f;
    public float jumpForce = 2f;
    public LayerMask groundLayer;
    public float detectionRange = 10f;

    [Header("Attack")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1f;
    public float attackKnockbackForce = 8f;
    public Transform attackPoint;
    public LayerMask playerLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool shouldJump;
    private float lastAttackTime;
    private Animator animator;

    private const string IS_WALKING = "isWalking";
    private const string ATTACK_TRIGGER = "attack";

    public int damage = 1;
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Visual")]
    public Color flashColor = Color.red;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isFacingRight = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (Player == null)
        {
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, Player.position);
        if (distanceToPlayer > detectionRange)
        {
            return;
        }

        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 2f, groundLayer);

        float direction = Mathf.Sign(Player.position.x - transform.position.x);

        bool isPlayerAbove = Physics2D.Raycast(transform.position, Vector2.up, 1.5f, 1 << Player.gameObject.layer);

        bool isPlayerInRange = Vector2.Distance(transform.position, Player.position) <= attackRange;
        if (isPlayerInRange)
        {
            TryAttack();
        }

        // Flip sprite to face player
        if (direction > 0 && isFacingRight)
        {
            Flip();
        }
        else if (direction < 0 && !isFacingRight)
        {
            Flip();
        }

        if (isGrounded)
        {
            if (!isPlayerInRange)
            {
                rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
            }

            RaycastHit2D groundInFront = Physics2D.Raycast(transform.position, new Vector2(direction, 0), 2f, groundLayer);

            RaycastHit2D gapAhead = Physics2D.Raycast(transform.position + new Vector3(direction, 0, 0), Vector2.down, 2f, groundLayer);

             RaycastHit2D platformAbove = Physics2D.Raycast(transform.position, Vector2. up, 3f, groundLayer);

             if (!groundInFront.collider && !gapAhead.collider) {
                    shouldJump = true;
            }
                else if (isPlayerAbove && platformAbove.collider)
            {
                shouldJump = true;
            }
                   
        }

        if (animator != null)
        {
            animator.SetBool(IS_WALKING, Mathf.Abs(rb.linearVelocity.x) > 0.1f && isGrounded);
        }
    }

    private void FixedUpdate()
    {
        if (isGrounded && shouldJump)
        {
            shouldJump = false;
            
            Vector2 direction = (Player.position - transform.position).normalized;

            Vector2 jumpDirection = direction * jumpForce;

            rb.AddForce(new Vector2(jumpDirection.x, jumpForce),ForceMode2D.Impulse) ;
           
        }
    }

    private void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;

        if (animator != null)
        {
            animator.SetTrigger(ATTACK_TRIGGER);
        }

        Vector2 origin = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        Collider2D playerHit = Physics2D.OverlapCircle(origin, attackRange, playerLayer);
        if (playerHit != null)
        {
            PlayerHealth playerHealth = playerHit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Vector2 knockbackDirection = (playerHit.transform.position - transform.position).normalized;
                playerHealth.ApplyDamage(damage, knockbackDirection, attackKnockbackForce);
            }
        }
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection, float knockbackForce)
    {
        currentHealth -= damage;
        Debug.Log("Enemy Take Damage - Current Health: " + currentHealth + ", SpriteRenderer: " + (spriteRenderer != null));
        StartCoroutine(FlashEffect());

        // Apply knockback
        rb.linearVelocity = new Vector2(knockbackDirection.x * knockbackForce, knockbackForce * 0.5f);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashEffect()
    {
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer is null in FlashEffect");
            yield break;
        }
        
        Color tempOriginal = spriteRenderer.color;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(0.25f);
        spriteRenderer.color = tempOriginal;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 origin = attackPoint != null ? attackPoint.position : transform.position;
        Gizmos.DrawWireSphere(origin, attackRange);
    }
}
