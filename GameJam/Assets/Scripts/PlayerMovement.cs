using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Component References")]
    [SerializeField] private Rigidbody2D playerRigidbody;

    [Header("Player Movement Settings")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] LayerMask groundLayer;

    private float horizontal;

    private void FixedUpdate()
    {
        // Handle horizontal movement
        playerRigidbody.linearVelocity = new Vector2(horizontal * moveSpeed, playerRigidbody.linearVelocity.y);
    }

    #region PLAYER_CONTROLLER
    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded())
        {
            playerRigidbody.linearVelocity = new Vector2(playerRigidbody.linearVelocity.x, jumpForce);
        }
    }
    private bool IsGrounded()
    {
        return Physics2D.OverlapCapsule(groundCheck.position, new Vector2(1f, 0.1f), CapsuleDirection2D.Horizontal, 0f, groundLayer);
    }

    #endregion

}