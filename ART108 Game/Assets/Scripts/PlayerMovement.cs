using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Rigidbody2D rb;
    public float moveSpeed = 5f;

    bool isFacingRight = true;
    

    float horizontalMovement;

    [UnitHeaderInspectable("Jumping")]
    public float jumpPower = 10f;
    public int maxJumps = 2;
    int jumpsRemaining;

    public bool lightMask;


    [UnitHeaderInspectable("Ground Check")]
    public Transform groundCheckPos;
    public Vector2 groundcheckSize = new Vector2(0.8f, 0.05f);
    public LayerMask groundLayer;
    bool isGrounded;



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


    [Header("Gravity")]
    public float baseGravity = 1f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
        GroundCheck();
        Gravity();
        ProcessWallSlide();
        ProcessWallJump();
              
        if (!isWallJumping) {
         rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);
         Flip();
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
              //  Debug.Log("normal juump");
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
                jumpsRemaining--;
            } else if (context.canceled && rb.linearVelocity.y > 0)
            {
             //    Debug.Log("Message here");
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
                jumpsRemaining--;
            }
        }

        // Wall Jump

        if (context.performed && wallJumpTimer  > 0f) {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
            wallJumpTimer = 0;

            if(transform.localScale.x != wallJumpDirection)
            {
                Debug.Log("Wall Jump");
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            Invoke(nameof(CancelWallJump), wallJumpTime + 0.1f);
        }
    }

    private void GroundCheck()
    {
        if(Physics2D.OverlapBox(groundCheckPos.position, groundcheckSize, 0f, groundLayer))
        {
            jumpsRemaining = maxJumps;
            isGrounded = true;
        }
        isGrounded = false;
        
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
