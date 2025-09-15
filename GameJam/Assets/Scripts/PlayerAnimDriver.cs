using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnimDriver : MonoBehaviour
{
    [SerializeField] Animator animator;   // drag the Animator from Graphics here
    [SerializeField] Rigidbody2D rb;      // drag Player’s Rigidbody2D here

    void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (animator == null || rb == null) return;

        float speed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("Speed", speed);
    }
}
