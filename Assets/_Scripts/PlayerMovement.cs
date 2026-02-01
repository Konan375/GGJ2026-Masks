using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    public Rigidbody2D rb;
    public float movespeed = 5f;
    public float jumpforce = 10f;

    [Header("Wall Slide Settings")]
    public float wallSlidingSpeed = 2f;
    [Range(0f, 1f)] public float wallClimbDamping = 0.5f;
    [SerializeField] private bool isWallSliding;

    [Header("Grounded Settings")]
    [SerializeField] private float hangTime = 0.2f;
    private float hangTimeCounter;
    public Transform groundCheck;
    public Transform WallCheckR;
    public Transform WallCheckL;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Jumping Settings")]
    public int extraJumpsValue = 1;
    public int extraJumps;
    public bool isGrounded;
    public bool isTouchingWall;

    [Header("Input References")]
    public InputActionReference move;
    public InputActionReference jump;

    private Vector2 _moveDirection;
    private Vector2 _jumpDirection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        extraJumps = extraJumpsValue;
    }

    // Update is called once per frame
    void Update()
    {
        _moveDirection = move.action.ReadValue<Vector2>();
        if (jump.action.WasPressedThisFrame())
        {
            HandleJump();
        }

    }

    private void FixedUpdate()
    {
        CheckStatus();
        Vector2 currentVel = rb.linearVelocity;

        currentVel.x = _moveDirection.x * movespeed;
        print(IsPushingAgainstWall());
        if (IsPushingAgainstWall()) {
            if (currentVel.y < 0)
            {
                isWallSliding = true;
                currentVel.y = Mathf.Max(currentVel.y, -wallSlidingSpeed);
            }
            else if (currentVel.y > 0)
            {

                isWallSliding = false;
                currentVel.y *= wallClimbDamping;
            }
        }
        else
            isWallSliding = false;
        rb.linearVelocity = currentVel;

    }
    
    private bool IsPushingAgainstWall()
    {
        if (isGrounded)
            return false;
        bool isTouchingRight = Physics2D.OverlapCircle(WallCheckR.position, groundCheckRadius, groundLayer);
        bool isTouchingLeft = Physics2D.OverlapCircle(WallCheckL.position, groundCheckRadius, groundLayer);
        return (isTouchingRight && _moveDirection.x >0.1f) || (isTouchingLeft && _moveDirection.x < -0.1f);
    }

    private void HandleJump()
    {
        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpforce);
            isWallSliding = false;
        }
        if (hangTimeCounter > 0f)
        {
            ApplyJumpForce();
        }
        else if (extraJumps > 0)
        {
            extraJumps--;
            ApplyJumpForce();
        }
    }

    private void ApplyJumpForce()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpforce);
    }

    private void CheckStatus()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            hangTimeCounter = hangTime;
            extraJumps = extraJumpsValue;
        }
        else
        {
            hangTimeCounter -= Time.deltaTime;
        }

    }
}
