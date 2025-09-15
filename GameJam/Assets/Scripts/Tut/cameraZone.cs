using UnityEngine;

public class CameraYOffsetZone2D : MonoBehaviour
{
    [SerializeField] private CameraController cam;
    [SerializeField] private float offsetWhileInside = 2f;
    [SerializeField] private float enterDuration = 0.35f;
    [SerializeField] private float exitDuration = 0.35f;
    [SerializeField] private string playerTag = "Player";

    private bool active;
    private float previousOffset;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag) || cam == null) return;

        // If already active, do nothing (prevents stacking)
        if (active) return;

        previousOffset = cam.GetYOffset();                 // requires your GetYOffset() helper
        cam.SetYOffsetSmooth(offsetWhileInside, enterDuration);
        active = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag) || cam == null) return;
        if (!active) return;

        cam.SetYOffsetSmooth(previousOffset, exitDuration);
        active = false;
    }
}
