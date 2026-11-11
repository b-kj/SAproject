using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private bool smoothMovement = true;
    [SerializeField] private float movementSmoothing = 0.08f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float fallGravityMultiplier = 2.5f;
    [SerializeField] private float lowJumpGravityMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;
    [SerializeField] private float jumpGaze = 3;
    [SerializeField] private float currnetjumpGaze = 0;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Jump")]
    [SerializeField] private float wallCheckDistance = 0.6f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(10f, 14f);
    [SerializeField] private float wallClingDrain = 1f;
    [SerializeField] private float wallJumpCost = 2f; 
    [SerializeField] private float wallCheckHeight = 1f;
    [SerializeField] private float wallJumpKnockback = 5f;
    public float direction = 1;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private Vector2 velocitySmoothing;
    private PlayerCombat playerCombat;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private bool isTouchingWall;
    private float wallDirection;
    private bool isWallClinging;
    private float originalGravityScale;
    private bool wasPressingWall;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCombat = GetComponent<PlayerCombat>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currnetjumpGaze = jumpGaze;
        originalGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        anim.SetFloat("Speed", Mathf.Abs(horizontalInput));

        if (!playerCombat.slowMotion)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            if (horizontalInput != 0)
            {
                direction = horizontalInput;
            }

            if (Input.GetButtonDown("Jump"))
            {
                jumpBufferCounter = jumpBufferTime;
            }
            else
            {
                jumpBufferCounter -= Time.deltaTime;
            }
        }

        ApplyJumpGravityModifiers();
        UpdateFacingDirection();
    }

    private void FixedUpdate()
    {
        isGrounded = CheckIsGrounded();
        CheckIsTouchingWall();

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            anim.SetBool("isJump", false);
            currnetjumpGaze = jumpGaze;
            isWallClinging = false;

        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
            HandleWallCling();
        }

        HandleMovement();
        HandleJump();
    }

    private void HandleMovement()
    {
        if (playerCombat.IsKnockback) return;
        float targetVelocityX = horizontalInput * moveSpeed;


        if (smoothMovement)
        {
            Vector2 targetVelocity = new Vector2(targetVelocityX, rb.linearVelocity.y);

            rb.linearVelocity = Vector2.SmoothDamp(
                rb.linearVelocity,
                targetVelocity,
                ref velocitySmoothing,
                movementSmoothing
            );
        }
        else
        {
            rb.linearVelocity = new Vector2(targetVelocityX, rb.linearVelocity.y);
        }
    }

    private void HandleJump()
    {
        if (jumpBufferCounter <= 0f)
            return;

        // 벽에 붙어 있거나 벽 캐스트가 감지 중이면 벽 점프
        if (wasPressingWall && coyoteTimeCounter > 0 && currnetjumpGaze >= wallJumpCost)
        {
            currnetjumpGaze -= wallJumpCost;

            rb.gravityScale = originalGravityScale;
            rb.linearVelocity = Vector2.zero;

            // 바라보는 방향 반전
            direction = -wallDirection;
            spriteRenderer.flipX = direction < 0;

            // 벽 반대 방향으로 튕겨 나가기
            rb.AddForce(
                new Vector2(
                    direction * wallJumpKnockback,
                    wallJumpForce.y
                ),
                ForceMode2D.Impulse
            );

            isWallClinging = false;
            wasPressingWall = false;
            coyoteTimeCounter = 0f;

            velocitySmoothing = Vector2.zero;

            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;

            anim.SetBool("isJump", true);
            return;
        }

        // 일반 점프
        if (coyoteTimeCounter <= 0f)
            return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
        isGrounded = false;

        anim.SetBool("isJump", true);
    
    }

    private void ApplyJumpGravityModifiers()
    {
        if (isWallClinging)
            return;

        if (Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpGravityMultiplier - 1f) * Time.deltaTime;
        }
        else
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallGravityMultiplier - 1f) * Time.deltaTime;
        }
    }

    private bool CheckIsGrounded()
    {
        if (groundCheck == null)
        {
            return false;
        }

        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
    }

    private void CheckIsTouchingWall()
    {
        Vector2 checkPosition = (Vector2)transform.position + Vector2.up * wallCheckHeight;

        Vector2 checkDirection = Vector2.right * direction;

        RaycastHit2D hit = Physics2D.Raycast(checkPosition,checkDirection, wallCheckDistance, groundLayer);

        isTouchingWall = hit.collider != null;

        if (isTouchingWall)
        {
            wallDirection = direction;
        }

        Debug.DrawRay(
            checkPosition,
            checkDirection * wallCheckDistance,
            isTouchingWall ? Color.green : Color.red
        );
    }
    private void HandleWallCling()
    {
        bool pressingWallDirection = horizontalInput * direction > 0;

        if (isTouchingWall && pressingWallDirection && currnetjumpGaze > 0)
        {
            coyoteTimeCounter = coyoteTime;
            wasPressingWall = true;
        }
        else if (wasPressingWall && coyoteTimeCounter > 0)
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }
        else
        {
            wasPressingWall = false;
        }

        isWallClinging = wasPressingWall && coyoteTimeCounter > 0 && currnetjumpGaze > 0;

        if (isWallClinging)
        {
            currnetjumpGaze -= wallClingDrain * Time.fixedDeltaTime;
            currnetjumpGaze = Mathf.Max(0, currnetjumpGaze);

            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;

            if (currnetjumpGaze <= 0)
            {
                isWallClinging = false;
                wasPressingWall = false;
                coyoteTimeCounter = 0f;
                rb.gravityScale = originalGravityScale;
            }
        }
        else
        {
            rb.gravityScale = originalGravityScale;
        }
    }
    private void UpdateFacingDirection()
    {
        if (horizontalInput > 0.01f) spriteRenderer.flipX = false;
        else if (horizontalInput < -0.01f) spriteRenderer.flipX = true;
    }
}
