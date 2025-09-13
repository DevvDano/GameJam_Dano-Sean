using UnityEngine;

public class UFOTrigger : MonoBehaviour
{
    [SerializeField] private float upwardVelocity = 15f;
    public Transform toCameraPos;
    public Transform AlienCamera;
    public Camera cam;
    private float zoomVel;
    public float smoothTime = 2f;
    public float zoomSmoothTime = 2f;
    public float targetOrthoSize = 10f;

    private Vector3 camVelocity = Vector3.zero;
    private bool triggerHit = false;

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
        // Smooth camera follow after trigger (optional)
        if (triggerHit && toCameraPos != null && AlienCamera != null)
        {
            // Camera Movement
            Vector3 targetPosition = toCameraPos.TransformPoint(new Vector3(0, 5, -10));
            AlienCamera.position = Vector3.SmoothDamp(
                AlienCamera.position, targetPosition, ref camVelocity, smoothTime);

            // Camera Zoom
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetOrthoSize, ref zoomVel, zoomSmoothTime);

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
    
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        triggerHit = true;
    }
}
