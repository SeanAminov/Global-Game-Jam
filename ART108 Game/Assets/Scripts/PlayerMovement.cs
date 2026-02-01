using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Rigidbody2D rb;

    CapsuleCollider2D playerCollider;
    public float moveSpeed = 5f;
    public float joyMaskSpeedBoost = 1.5f;

    bool isFacingRight = true;
    public bool IsFacingRight => isFacingRight;
    

    float horizontalMovement;

    [UnitHeaderInspectable("Jumping")]
    public float jumpPower = 10f;
    public int maxJumps = 2;
    int jumpsRemaining;

    [Header("Jump Charge")]
    public float jumpChargeTime = 0.15f;

    public bool lightMask;

    [Header("Audio")]
    public AudioClip jumpSound;
    public AudioClip footstepSound;
    [Range(0f, 3f)]
    public float jumpVolume = 1f;
    [Range(0f, 3f)]
    public float footstepVolume = 2f;
    public float footstepInterval = 0.3f;  // Time between footstep sounds
    private float footstepTimer;
    public AudioClip punchSound;
    private AudioSource audioSource;

    [Header("Particles")]
    public ParticleSystem jumpParticles;


    [UnitHeaderInspectable("Ground Check")]
    public Transform groundCheckPos;
    public Vector2 groundcheckSize = new Vector2(0.8f, 0.05f);
    public LayerMask groundLayer;
    public LayerMask platformLayer;
    bool isGrounded;
    bool isOnPlatform;
    private Collider2D currentPlatformCollider;



     [UnitHeaderInspectable("Wall Check")]
    public Transform wallCheckPos;
    public Vector2 wallcheckSize = new Vector2(0.8f, 0.05f);
    public LayerMask wallLayer;

    [Header("WallMovement")]
    public float wallSlideSpped = 2;
    bool isWallSliding;

    bool isWallJumping;
    float wallJumpDirection;
    float wallJumpTime = 0.5f;
    float wallJumpTimer;
    public Vector2 wallJumpPower = new Vector2(5f, 10f);

    [HideInInspector] public bool isAttacking;
    private PlayerAnimator playerAnimator;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;


    [Header("Gravity")]
    public float baseGravity = 1f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;


    void Start()
    {
        playerAnimator = GetComponent<PlayerAnimator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    // Update is called once per frame
    void Update()
    {
       
        GroundCheck();
        Gravity();
        ProcessWallSlide();
        ProcessWallJump();
        UpdateMaskTint();
        PlayFootsteps();
              
        if (!isWallJumping && !isAttacking) {
         float currentSpeed = moveSpeed;
         if (MaskManager.Instance != null && MaskManager.Instance.IsMaskEquipped(MaskType.Joy))
         {
             currentSpeed *= joyMaskSpeedBoost;
         }
         rb.linearVelocity = new Vector2(horizontalMovement * currentSpeed, rb.linearVelocity.y);
         Flip();
        }
    }

    private void PlayFootsteps()
    {
        // Only play footsteps when grounded, moving, and not attacking
        if (isGrounded && Mathf.Abs(horizontalMovement) > 0.1f && !isAttacking)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                if (audioSource != null && footstepSound != null)
                {
                    audioSource.PlayOneShot(footstepSound, footstepVolume);
                }
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            // Stop any playing footstep sound immediately
            footstepTimer = 0f;
            if (audioSource != null && audioSource.isPlaying && audioSource.clip == footstepSound)
            {
                audioSource.Stop();
            }
        }
    }

    private void UpdateMaskTint()
    {
        if (spriteRenderer == null || MaskManager.Instance == null)
        {
            return;
        }

        if (MaskManager.Instance.IsMaskEquipped(MaskType.Sad))
        {
            spriteRenderer.color = new Color(0.7f, 0.7f, 1f); // Sad blue tint
        }
        else if (MaskManager.Instance.IsMaskEquipped(MaskType.Joy))
        {
            spriteRenderer.color = Color.yellow;
        }
        else if (MaskManager.Instance.IsMaskEquipped(MaskType.Spite))
        {
            spriteRenderer.color = Color.red;
        }
        else
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void Gravity() {
        if (rb.linearVelocityY < 0) {
            rb.gravityScale = baseGravity * fallSpeedMultiplier;
            rb.linearVelocityY = Mathf.Max(rb.linearVelocity.y, -maxFallSpeed);
        } else {
            rb.gravityScale = baseGravity;
        }
    }
    
    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (jumpsRemaining > 0) {
            if (context.performed)
            {
                // Trigger jump animation immediately
                if (playerAnimator != null)
                {
                    playerAnimator.TriggerJump();
                }
                StartCoroutine(ChargeAndJump());
            } else if (context.canceled && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
                jumpsRemaining--;
            }
        }

        // Wall Jump

        if (context.performed && wallJumpTimer  > 0f) {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
            wallJumpTimer = 0;
            
            // Play jump particles
            if (jumpParticles != null)
            {
                jumpParticles.Play();
            }

            if(transform.localScale.x != wallJumpDirection)
            {
            //    Debug.Log("Wall Jump");
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            Invoke(nameof(CancelWallJump), wallJumpTime + 0.1f);
        }
    }

    public void Drop(InputAction.CallbackContext context)
    {
        if (context.performed && isOnPlatform && currentPlatformCollider != null)
        {
            StartCoroutine(TemporarilyIgnorePlatforms(0.5f));
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = true;
            currentPlatformCollider = collision.collider;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = false;
            currentPlatformCollider = null;
        }
    }

    private IEnumerator TemporarilyIgnorePlatforms(float disableTime)
    {
        if (currentPlatformCollider == null)
            yield break;

        // Get the Tilemap Collider 2D from the platform and disable it
        TilemapCollider2D tilemapCollider = currentPlatformCollider.GetComponent<TilemapCollider2D>();
        CompositeCollider2D compositeCollider = currentPlatformCollider.GetComponent<CompositeCollider2D>();
        
        if (tilemapCollider != null)
        {
            tilemapCollider.enabled = false;
        }
        if (compositeCollider != null)
        {
            compositeCollider.enabled = false;
        }
        
        yield return new WaitForSeconds(disableTime);
        
        if (tilemapCollider != null)
        {
            tilemapCollider.enabled = true;
        }
        if (compositeCollider != null)
        {
            compositeCollider.enabled = true;
        }
    }

    private System.Collections.IEnumerator ChargeAndJump()
    {
        yield return new WaitForSeconds(jumpChargeTime);
        
        // Play jump sound after windup
        if (audioSource != null && jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound, jumpVolume);
        }
        
        // Play jump particles
        if (jumpParticles != null)
        {
            jumpParticles.Play();
        }
        
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
        jumpsRemaining--;
    }

    private void GroundCheck()
    {
        if (Physics2D.OverlapBox(groundCheckPos.position, groundcheckSize, 0f, groundLayer))
        {
            jumpsRemaining = maxJumps;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void ProcessWallSlide() {
        if (!isGrounded & WallCheck() & horizontalMovement != 0) {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpped));
        } else {
            isWallSliding = false;
        }
    }

    private void ProcessWallJump() {
        if (isWallSliding) {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpTimer = wallJumpTime;

            CancelInvoke(nameof(CancelWallJump));
        } else if (wallJumpTimer > 0f) {
            wallJumpTimer -= Time.deltaTime;
        }
    }

    private void CancelWallJump()
    {
        isWallJumping = false;
    }

    public bool WallCheck() {
         return Physics2D.OverlapBox(wallCheckPos.position, wallcheckSize, 0, wallLayer);
       
    }

    public void ResetJumps()
    {
        jumpsRemaining = maxJumps;
    }


    private void Flip() {
        if(isFacingRight && horizontalMovement < 0 || !isFacingRight && horizontalMovement > 0) {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheckPos.position, groundcheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos.position, wallcheckSize);
    }
}
