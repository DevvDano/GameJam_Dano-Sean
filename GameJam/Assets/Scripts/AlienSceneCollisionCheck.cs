using UnityEngine;
public class CameraFocusTrigger2D : MonoBehaviour
{
    public Transform focusTarget;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Camera.main.GetComponent<Follow_player>().SetOverrideTarget(focusTarget);
        }
    }
}