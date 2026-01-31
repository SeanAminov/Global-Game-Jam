using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;
    private Rigidbody2D rb;

    // Animation parameter names
    private const string IS_WALKING = "isWalking";
    private const string IS_JUMPING = "isJumping";
    private const string IS_FALLING = "isFalling";
    private const string IS_ATTACKING = "isAttacking";
    private const string IS_WALL_SLIDING = "isWallSliding";

    [Header("Optional: For single-frame attack sprite")]
    public Sprite attackSprite;
    private SpriteRenderer spriteRenderer;

    private bool wasJumping = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAttack = GetComponent<PlayerAttack>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (animator == null) return;

        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        // Get movement values
        float horizontalInput = Mathf.Abs(rb.linearVelocity.x);
        float verticalVelocity = rb.linearVelocity.y;
        
        // Check if grounded (use ground check from PlayerMovement)
        bool isGrounded = Physics2D.OverlapBox(playerMovement.groundCheckPos.position, playerMovement.groundcheckSize, 0f, playerMovement.groundLayer);

        // Walking
        animator.SetBool(IS_WALKING, horizontalInput > 0.1f && isGrounded);

        // Falling - only set, jumping is triggered from PlayerMovement
        animator.SetBool(IS_FALLING, verticalVelocity < -0.1f && !isGrounded);

        // Wall Sliding
        bool isWallSliding = playerMovement.WallCheck() && !isGrounded && horizontalInput > 0.1f;
        animator.SetBool(IS_WALL_SLIDING, isWallSliding);
    }

    // Call this from PlayerAttack when attack happens
    public void TriggerAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger(IS_ATTACKING);
        }
        
        // Optional: If you set an attackSprite and don't have an attack animation,
        // this will briefly show the attack sprite
        if (attackSprite != null && spriteRenderer != null)
        {
            StartCoroutine(ShowAttackSprite());
        }
    }

    // Call this from PlayerMovement when jump starts
    public void TriggerJump()
    {
        if (animator != null)
        {
            animator.SetTrigger(IS_JUMPING);
        }
    }

    private System.Collections.IEnumerator ShowAttackSprite()
    {
        // This is only used if you don't have an attack animation set up
        // and want to manually swap to the attack sprite briefly
        Sprite originalSprite = spriteRenderer.sprite;
        spriteRenderer.sprite = attackSprite;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.sprite = originalSprite;
    }
}
