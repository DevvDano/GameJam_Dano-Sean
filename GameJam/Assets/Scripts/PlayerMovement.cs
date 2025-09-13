using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Rigidbody2D playerRigidbody;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(1f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    private float horizontal;
    private float coyoteCounter;
    private float jumpBufferCounter;

    void Reset() { playerRigidbody = GetComponent<Rigidbody2D>(); }

    void Update()
    {
        // coyote / buffer
        if (IsGrounded()) coyoteCounter = coyoteTime; else coyoteCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f)
        {
            if (coyoteCounter > 0f) { DoJump(); jumpBufferCounter = 0f; }
            else jumpBufferCounter -= Time.deltaTime;
        }

        // flip sprite
        if (horizontal > 0.1f) transform.localScale = new Vector3(1f, 1f, 1f);
        else if (horizontal < -0.1f) transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    void FixedUpdate()
    {
        playerRigidbody.linearVelocity = new Vector2(horizontal * moveSpeed, playerRigidbody.linearVelocity.y);
    }

    // === Input System (Send Messages expects method names == action names) ===
    public void Move(InputAction.CallbackContext ctx)
    {
        horizontal = ctx.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            jumpBufferCounter = jumpBufferTime;
            if (coyoteCounter > 0f) { DoJump(); jumpBufferCounter = 0f; }
        }
        else if (ctx.canceled)
        {
            if (playerRigidbody.linearVelocity.y > 0f)
                playerRigidbody.linearVelocity = new Vector2(playerRigidbody.linearVelocity.x, playerRigidbody.linearVelocity.y * 0.6f);
        }
    }

    // === Helpers ===
    private void DoJump()
    {
        playerRigidbody.linearVelocity = new Vector2(playerRigidbody.linearVelocity.x, 0f);
        playerRigidbody.linearVelocity += Vector2.up * jumpForce;
        coyoteCounter = 0f;
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCapsule(groundCheck.position, groundCheckSize, CapsuleDirection2D.Horizontal, 0f, groundLayer);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
#endif
}
