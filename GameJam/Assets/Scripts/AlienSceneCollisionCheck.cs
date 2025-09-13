using UnityEngine;

public class AlienSceneCollisionCheck : MonoBehaviour
{
    public GameObject alienSceneCamera;

    //On Player Collision 2D with Object tagged "Trigger", Switch Camera to Alien Scene Camera
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Camera.main.gameObject.SetActive(false);
            if (alienSceneCamera != null)
            {
                alienSceneCamera.SetActive(true);
            }
            else
            {
                Debug.LogWarning("AlienSceneCamera not found in the scene.");
            }
        }
    }
}
