using UnityEngine;

public class Projectile : MonoBehaviour
{
    //Destroys on collision with anything
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}
