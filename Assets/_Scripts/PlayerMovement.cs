using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    public Rigidbody2D rb;

    public float movespeed;

    private Vector2 _moveDirection;

    public InputActionReference move;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _moveDirection = move.action.ReadValue<Vector2>();
        
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(_moveDirection.x * movespeed, _moveDirection.y * movespeed);
    }
}
