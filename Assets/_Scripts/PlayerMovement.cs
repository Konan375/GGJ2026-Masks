using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    //For loading scenes from strings given here
    [Header("Level List")]
    public int currentLevelIndex = 0;
    public string[] levelNames = { "Swamp", "JAMLEV" };
   
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
    [Header("Grounded Settings")]
    [SerializeField] private float hangTime = 0.2f;
    private float hangTimeCounter;
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
    /// <summary>
    /// Float for the circle cast from transforms for checking if player is 
    /// touching walls or ground
    /// </summary>
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    /// <summary>
    /// How high the player jumps
    /// </summary>
    [Header("Jumping Settings")]
    public float jumpforce = 10f;
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
    /// <summary>
    /// Checkks for if the player is pressing the movement button into the wall
    /// and sends back true or false
    /// </summary>
    /// <returns></returns>
    private bool IsPushingAgainstWall()
    {
        if (isGrounded)
            return false;
        bool isTouchingRight = (Physics2D.OverlapCircle(WallCheckR.position, groundCheckRadius, groundLayer) && !isTouchingIce);
        bool isTouchingLeft = (Physics2D.OverlapCircle(WallCheckL.position, groundCheckRadius, groundLayer) && !isTouchingIce);
        return (isTouchingRight && _moveDirection.x >0.1f) || (isTouchingLeft && _moveDirection.x < -0.1f);
    }
    /// <summary>
    /// Checks current varibles to ensure that the player can jump if the button is pressed
    /// </summary>
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
    /// <summary>
    /// Uses the jump variables to make the player jump in the air/
    /// </summary>
    private void ApplyJumpForce()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpforce);
    }
    /// <summary>
    /// Checks tile player is standing on to determine if the player is able to jump
    /// </summary>
    private void CheckStatus()
    {
        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (hit != null)
        {
            Vector3Int gridPos = landTileMap.WorldToCell(groundCheck.position);
            TileBase tile = landTileMap.GetTile(gridPos);

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
        isTouchingIce = false;

    }

}
