using UnityEngine;

public class Follow_player : MonoBehaviour
{
    [Header("Default Target")]
    public Transform player;   // Player is the normal follow target

    [Header("Fixed Camera Settings")]
    public bool useFixedY = true;  // Toggle to lock/unlock Y
    public float fixedY = 4.25f;
    public float fixedZ = -10f;

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 5f; // higher = snappier, lower = smoother

    private Transform overrideTarget;

    void LateUpdate()
    {
        if (!player) return;

        // Use override target if one exists, otherwise follow the player
        Transform target = overrideTarget != null ? overrideTarget : player;

        float targetY = useFixedY ? fixedY : target.position.y;

        // Smooth only the Y (X snaps directly to target)
        float smoothedY = Mathf.Lerp(transform.position.y, targetY, smoothSpeed * Time.deltaTime);

        transform.position = new Vector3(
            target.position.x,
            smoothedY,
            fixedZ
        );
    }

    // Methods to control the override
    public void SetOverrideTarget(Transform newTarget) => overrideTarget = newTarget;
    public void ClearOverrideTarget() => overrideTarget = null;
}
