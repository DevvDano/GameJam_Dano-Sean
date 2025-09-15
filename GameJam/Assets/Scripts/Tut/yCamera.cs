using UnityEngine;

public class DisableFollowYTrigger : MonoBehaviour
{
    [SerializeField] private CameraController cam;
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag) || cam == null) return;
        cam.LockCurrentY();  // disables followY and freezes current Y
    }
}
