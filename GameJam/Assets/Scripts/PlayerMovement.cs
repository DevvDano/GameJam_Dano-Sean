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

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileForce;

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

    //Flip the player sprite based on movement direction
    private void LateUpdate()
    {
        if (horizontal > 0.1f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (horizontal < -0.1f)
            transform.localScale = new Vector3(-1, 1, 1);
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

    #region PLAYER_COMBAT

    public void FireProjectile(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
            float direction = Mathf.Sign(transform.localScale.x); // check player facing
            Vector2 shootDir = new Vector2(direction, 0);
            projectileRb.AddForce(shootDir * projectileForce, ForceMode2D.Impulse);
            Destroy(projectile, 5f); // Destroy projectile after 5 seconds
            Debug.Log("Projectile Fired");
        }
    }
    
    public void MeleeAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Implement melee attack logic here
            Debug.Log("Melee Attack Executed");
        }
    }

    #endregion
}