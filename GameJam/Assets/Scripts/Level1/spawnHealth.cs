using UnityEngine;
public class SpawnOnCollision2D : MonoBehaviour {
    [SerializeField] private GameObject prefabHealth;
    [SerializeField] private float spawnOffsetY = 1f;
    [SerializeField] private Transform objReference; // If the object has "Is Trigger" checked on its Collider2D
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player"))
        {
            Vector3 spawnPos = objReference.position + new Vector3(0, spawnOffsetY, 0);
            Instantiate(prefabHealth, spawnPos, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}