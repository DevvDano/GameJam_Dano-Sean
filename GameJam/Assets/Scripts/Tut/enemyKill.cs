using UnityEngine;

public class DestroyOnEnemyTrigger : MonoBehaviour
{
    [SerializeField] private string enemyLayerName = "Enemy";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(enemyLayerName))
        {
            // Destroy the enemy
            Destroy(other.gameObject);

            // Destroy this object too
            Destroy(gameObject);
        }
    }
}
