using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class BobAndLoadScene : MonoBehaviour
{
    [Header("Bobbing Settings")]
    [SerializeField] private float bobHeight = 0.5f; // how high it moves
    [SerializeField] private float bobSpeed = 2f;    // how fast it bobs

    [Header("Scene Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private int sceneOffset = 1; // load next scene (1 = next, -1 = previous, etc.)

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;

        // Ensure trigger collider
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Update()
    {
        // Bobbing motion
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + sceneOffset;

            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.LogWarning("No next scene in Build Settings!");
            }
        }
    }
}
