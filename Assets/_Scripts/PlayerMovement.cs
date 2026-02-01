using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public Object nextScene;
    public Rigidbody2D rb;
    public float movespeed = 5f;
    public float jumpforce = 10f;
    public Tilemap itemTileMap;
    [Header("Ice Settings")]
    public float iceFriction = 2f;
    public float normalFriction = 20f;
    private float currentFriction;

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
        currentFriction = normalFriction;
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

        float targetX = _moveDirection.x * movespeed;

        currentVel.x = Mathf.Lerp(currentVel.x, targetX, currentFriction * Time.fixedDeltaTime);
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
        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (hit != null)
        {
            Vector3Int gridPos = itemTileMap.WorldToCell(groundCheck.position);
            TileBase tile = itemTileMap.GetTile(gridPos);

            if (tile is CustomDataTile data)
            {
                isGrounded = (data.types == Tiles.Ground || data.types == Tiles.Slippery);

            }
            else
            {
                isGrounded = true;
            }
        }

        else isGrounded = false;



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

    private void OnCollisionStay2D(Collision2D collision)
    {

        //print("collided!");

        Vector3 hitpoint = collision.contacts[0].point;
        Vector3Int gridPos = itemTileMap.WorldToCell(hitpoint - (Vector3)collision.contacts[0].normal *0.1f);   
        TileBase tile = itemTileMap.GetTile(gridPos);
        if (tile is CustomDataTile data)
        {
            HandleTileLogic(data.types, itemTileMap, gridPos);
        }

    }
    void HandleTileLogic(Tiles type, Tilemap tilemap, Vector3Int gridPos)
    {
        print("hello!");
        switch (type)
        {
            case Tiles.Slippery:
                print("Slippy");
                currentFriction = iceFriction;
                break;
            case Tiles.Collectible:
                tilemap.SetTile(gridPos, null);
                break;
            case Tiles.Spikes:
                transform.position = transform.parent.position;
                break;
            case Tiles.End:
                SceneManager.LoadScene(nextScene.name);
                break;

        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        currentFriction = normalFriction;
    }
}
