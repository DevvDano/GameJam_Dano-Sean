using UnityEngine;

public class upwardTriggerPole : MonoBehaviour
{
    [SerializeField] private float upwardVelocity = 15f;

    public bool triggerHit = false;

    private GameObject player;
    private Rigidbody2D playerRigidbody;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            playerRigidbody = player.GetComponent<Rigidbody2D>();
        else
            Debug.LogWarning("Player GameObject not found!");
    }

    private void Update()
    {
        if (triggerHit)
            // Set player's vertical velocity
            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = new Vector2(playerRigidbody.linearVelocity.x, upwardVelocity);
            }
            else
            {
                Debug.LogWarning("Rigidbody2D component not found on the Player.");
            }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        triggerHit = true;
    }

    public void downwardTrigger()
    {
        triggerHit = false;
    }
}
