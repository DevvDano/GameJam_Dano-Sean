using UnityEngine;

public class PatrolOnCollision : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 2f; // seconds to wait before flipping allowed
    [SerializeField] private string groundLayerName = "Ground"; // layer to ignore

    private int direction = 1; 
    private Rigidbody2D rb;
    private float spawnTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        spawnTime = Time.time; // record when we spawned
    }

    private void Update()
    {
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Don’t flip until waitTime seconds have passed
        if (Time.time - spawnTime < waitTime) return;

        // Skip flipping if the object is on the Ground layer
        if (collision.gameObject.layer == LayerMask.NameToLayer(groundLayerName))
            return;

        direction *= -1;

        // Optional: flip sprite visually
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }
}
