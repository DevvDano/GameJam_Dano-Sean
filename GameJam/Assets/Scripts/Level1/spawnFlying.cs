using UnityEngine;

public class SpawnAndDestroyTrigger : MonoBehaviour
{
    [SerializeField] private GameObject prefabToSpawn;  // prefab to create
    [SerializeField] private Transform spawnPos;        // where to spawn prefab
    [SerializeField] private string targetTag = "Player"; // who can activate trigger

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only react to the target (default: Player)
        if (!other.CompareTag(targetTag)) return;

        // Spawn prefab at spawnPos location
        if (prefabToSpawn && spawnPos)
        {
            Instantiate(prefabToSpawn, spawnPos.position, Quaternion.identity);
        }

        // Destroy this trigger object
        Destroy(gameObject);
    }
}
