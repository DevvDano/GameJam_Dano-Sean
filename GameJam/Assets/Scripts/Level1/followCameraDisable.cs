using UnityEngine;

public class CameraYToggleTrigger : MonoBehaviour
{
    [SerializeField] private Follow_player cameraFollow; // drag your Camera with Follow_player here
    [SerializeField] private bool enableFixedY = false;  // what to set when triggered
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (cameraFollow != null)
        {
            cameraFollow.useFixedY = enableFixedY;
        }
        else
        {
            Debug.LogWarning("Camera Follow script not assigned on trigger!", this);
        }
    }
}
