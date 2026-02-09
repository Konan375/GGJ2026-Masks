using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System;

public class PlayerMovement : MonoBehaviour
{
    //For loading scenes from strings given here
    [Header("Level List")]
    public int currentLevelIndex = 0;
    public string[] levelNames = { "Swamp", "JAMLEV" };
    public float wallJumpXStrength;
    private List<Vector3Int> collectedTilesPositions = new List<Vector3Int>();
    public TileBase collectibleTile;
    // Basic movement and physics variables
    public Rigidbody2D rb;
    public float movespeed = 5f;
    /// <summary>
    /// Tilemaps to interact with. Land for terrian, and items for Mask and Goal
    /// </summary>
    [Header("TileMaps")]
    public Tilemap landTileMap;
    public Tilemap itemTileMap;
    [Header("Grounded Settings")]
    /// <summary>
    /// transform for determining if the player is touching the ground
    /// </summary>
    public Transform groundCheck;
    /// <summary>
    /// Transform for determining if the player is touching a wall to the right
    /// </summary>
    public Transform WallCheckR;
    /// <summary>
    /// Transform for determining if the player is touching a wall to the left
    /// </summary>
    public Transform WallCheckL;
    [SerializeField] private float hangTime = 0.2f;
    private float hangTimeCounter;
    /// <summary>
    /// Float for the circle cast from transforms for checking if player is 
    /// touching walls or ground
    /// </summary>
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    [Header("Jumping Settings")]
    public float jumpHeightInTiles = 10f;
    public float GravityScale = 2;
    public int extraJumpsValue = 1;
    /// <summary>
    /// takes information from extraJumpsValue to jump double time
    /// </summary>
    public int extraJumps;
    /// <summary>
    /// Is the player on the ground
    /// </summary>
    public bool isGrounded;
    /// <summary>
    /// is the player touching the wall
    /// </summary>
    public bool isTouchingWall;

    public bool isWallJumping;
    public float walljump;
    public float wallJumpTimer;

    private int wallDir;
    /// <summary>
    /// Variables for ice interaction
    /// </summary>
    [Header("Ice Settings")]
    public float iceFriction = 2f;
    public float normalFriction = 20f;
    public bool isTouchingIce;
    private float smootheFriction;
    private float frictionVelocity;
    /// <summary>
    /// Variable for web interaction
    /// </summary>
    [Header("Web Settings")]
    public float stickyFriction = 40f;
    /// <summary>
    /// Variables for wall sliding
    /// TODO add ice interaction here
    /// </summary>
    [Header("Wall Slide Settings")]
    public float wallSlidingSpeed = 2f;
    [Range(0f, 1f)] public float wallClimbDamping = 0.5f;
    [SerializeField] private bool isWallSliding;
    /// <summary>
    ///Maximum amount of time that player can jump after leaving edge
    /// </summary>
    /// <summary>
    /// How high the player jumps
    /// </summary>
    /// <summary>
    /// Variables for keyboard control
    /// </summary>
    [Header("Input References")]
    public InputActionReference move;
    public InputActionReference jump;
    public InputActionReference escape;
    /// <summary>
    /// Private Variables for moving and jumping
    /// </summary>
    private Vector2 _moveDirection;
    private Vector2 _jumpDirection;
    private float currentFriction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Sets the extra jumps variable to what is put down
        extraJumps = extraJumpsValue;
        //Sets friction to normal
        currentFriction = normalFriction;
    }

    // Update is called once per frame
    void Update()
    {
        //Uses keybindings to check if movement direction is pressed, ie. left or right
        _moveDirection = move.action.ReadValue<Vector2>();
        // Checks for jump action
        if (jump.action.WasPressedThisFrame())
        {

            HandleJump();
        }
        //Checks for quit button
        if (escape.action.triggered) 
        {
            Application.Quit();
        }
    }

    private void FixedUpdate()
    {
        smootheFriction = Mathf.Lerp(smootheFriction, currentFriction, 5f * Time.fixedDeltaTime);
        CheckStatus();
        ///Sets variable for the current speed to act upon 
        Vector2 currentVel = rb.linearVelocity;
        // sets the movement speed based on the direction pressed,
        // but doesnt change it until later
        float targetX = _moveDirection.x * movespeed;
        if (!isWallJumping)
        {
            ///this sets the velocity depending on the tile player is standing on.
            if (isGrounded)
            {
                currentVel.x = Mathf.Lerp(currentVel.x, targetX, smootheFriction * Time.fixedDeltaTime);

            }

            else
            {
                float airControlAbility = 0.2f;
                currentVel.x = Mathf.Lerp(currentVel.x, targetX, (smootheFriction * airControlAbility) * Time.fixedDeltaTime);
            }
        }

        else
        {
            wallJumpTimer -= Time.fixedDeltaTime;
            isWallJumping = wallJumpTimer > 0f;
        }
        if (IsPushingAgainstWall()) {
            if (currentVel.y < 0)
            {
                isWallSliding = true;
                currentVel.y = Mathf.Max(currentVel.y, -wallSlidingSpeed);
                wallJumpTimer = walljump;
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
    /// <summary>
    /// Checkks for if the player is pressing the movement button into the wall
    /// and sends back true or false
    /// </summary>
    /// <returns></returns>
    private bool IsPushingAgainstWall()
    {
        if (isGrounded)
            return false;
        bool wallRight = IsTileIceAtPosition(WallCheckR.position, out bool iceRight);
        bool wallLeft = IsTileIceAtPosition(WallCheckL.position, out bool iceLeft);

        bool pushingRight = wallRight && _moveDirection.x > 0.1f && !iceRight;
        bool pushingLeft = wallLeft && _moveDirection.x < -0.1f && !iceLeft;
        wallDir = pushingLeft ? 1 : (pushingRight ? -1 : 0);
        return pushingLeft || pushingRight;
    }
///TODO: Put in tile script
    private bool IsTileIceAtPosition(Vector3 checkPos, out bool isIce)
    {
        isIce = false;
        Collider2D hit = Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundLayer);
        if (hit != null)
        {
            Vector3Int gridPos = landTileMap.WorldToCell(checkPos);
            TileBase tile = landTileMap.GetTile(gridPos);
            if (tile is CustomDataTile data)
            {
                isIce = (data.types == Tiles.Slippery);
            }
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Checks current varibles to ensure that the player can jump if the button is pressed
    /// </summary>
    private void HandleJump()
    {
        isTouchingIce = false;
        if (isWallSliding)
        {
            ApplyWallJumpForce();
            return;
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
    /// <summary>
    /// Uses the jump variables to make the player jump in the air/
    /// </summary>
    private void ApplyJumpForce()
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        float jumpVelocity = MathF.Sqrt(2 * gravity * jumpHeightInTiles);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
    }

    private void ApplyWallJumpForce()
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        float jumpVelocity = MathF.Sqrt(2 * gravity * jumpHeightInTiles);
        wallJumpXStrength = movespeed * 1.5f ;
        rb.linearVelocity = new Vector2(wallDir * movespeed, jumpVelocity);
        isWallJumping = true;
    }
    /// <summary>
    /// Checks tile player is standing on to determine if the player is able to jump
    /// </summary>
   private void CheckStatus()
    {
        Vector2 checkPos = (Vector2)groundCheck.position + Vector2.down * 0.1f;
        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (hit != null)
        {
            Vector3Int gridPos = landTileMap.WorldToCell(checkPos);
            TileBase tile = landTileMap.GetTile(gridPos);

            if (tile is CustomDataTile data)
            {
                isGrounded = (data.types == Tiles.Ground || data.types == Tiles.Slippery);

            }
            else if (tile != null) 
            {
                isGrounded = true;
                currentFriction = normalFriction;
            }
        }

        else
        {
            isTouchingIce = false;
            isGrounded = false;
        }



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


        Vector3 hitpoint = collision.contacts[0].point;
        Vector3Int gridPos = landTileMap.WorldToCell(hitpoint - (Vector3)collision.contacts[0].normal *0.1f);   
        TileBase tile = landTileMap.GetTile(gridPos);
        if (tile is CustomDataTile data)
        {
            HandleTileLogic(data.types, landTileMap, gridPos);
        }

    }
    void HandleTileLogic(Tiles type, Tilemap tilemap, Vector3Int gridPos)
    {
        switch (type)
        {
            case Tiles.Ground:
                isTouchingIce = false;
                currentFriction = normalFriction;
                break;
            case Tiles.Slippery:
                isTouchingIce = true;
                currentFriction = iceFriction;
                break;
            case Tiles.Sticky:
                currentFriction = stickyFriction;
                break;
            case Tiles.Collectible:
                if(!collectedTilesPositions.Contains(gridPos))
                {
                    collectedTilesPositions.Add(gridPos);
                }
                tilemap.SetTile(gridPos, null);
                break;
            case Tiles.Spikes:
                currentFriction = normalFriction;
                isTouchingIce = false;
                RespawnPlayer();
                break;
            case Tiles.End:
                print("You Win");
                int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;

                int nextIndex = (currentBuildIndex == 0) ? 1 : 0;;
                SceneManager.LoadScene(nextIndex);
                break;

        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        Vector3Int gridPos = itemTileMap.WorldToCell(transform.position);
        TileBase tile = itemTileMap.GetTile(gridPos);
        //print(tile);
        if (tile is CustomDataTile data && (data.types == Tiles.End ||
            data.types == Tiles.Collectible))
        {
            HandleTileLogic(data.types, itemTileMap, gridPos);
        }
    }
    private void RespawnPlayer()
    {
        transform.position = transform.parent.position;
        foreach (Vector3Int pos in collectedTilesPositions)
        {
            itemTileMap.SetTile(pos, collectibleTile);
        }

        collectedTilesPositions.Clear();
        rb.linearVelocity = Vector2.zero;

    }
    private void OnDrawGizmos()
    {
        // 1. Set the color for the Ground Check
        Gizmos.color = isGrounded ? Color.green : Color.red;
        if (groundCheck != null)
        {
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            Gizmos.DrawLine(groundCheck.position, (groundCheck.position + Vector3.down * 0.1f));
        }

        // 2. Set the color for Wall Checks
        Gizmos.color = Color.blue;
        if (WallCheckR != null)
        {
            Gizmos.DrawWireSphere(WallCheckR.position, groundCheckRadius);
        }
        if (WallCheckL != null)
        {
            Gizmos.DrawWireSphere(WallCheckL.position, groundCheckRadius);
        }
    }

    private void OnDrawGizmosSelected()
    {

        // 1. Setup the physics constants
        // We'll assume wallDir is 1 (jumping right) for the preview
        float previewDir = 1f;
        float g = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        float vY = Mathf.Sqrt(2 * g * jumpHeightInTiles);
        float vX = movespeed * previewDir;

        // 2. Calculate total time of the jump (up and back down to start height)
        // Formula: t = (2 * vY) / g
        float totalTime = (2 * vY) / g;
        int segments = 15; // How many dots/lines to draw
        Vector3 previousPoint = transform.position;

        Gizmos.color = Color.yellow;

        for (int i = 1; i <= segments; i++)
        {
            // Calculate time at this segment
            float t = (totalTime / segments) * i;

            // Kinematic formulas for X and Y position
            float x = vX * t;
            float y = (vY * t) - (0.5f * g * t * t);

            Vector3 nextPoint = transform.position + new Vector3(x, y, 0);

            // Draw the segment
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }

        // 3. Draw a little marker at the peak
        Gizmos.color = Color.cyan;
        float timeToPeak = vY / g;
        float peakX = vX * timeToPeak;
        Vector3 peakPos = transform.position + new Vector3(peakX, jumpHeightInTiles, 0);
        Gizmos.DrawWireSphere(peakPos, 0.2f);
    }
}
