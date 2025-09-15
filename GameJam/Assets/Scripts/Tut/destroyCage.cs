using UnityEngine;

public class destroyCage : MonoBehaviour
{
    // On Collision with player, destroy object
    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
