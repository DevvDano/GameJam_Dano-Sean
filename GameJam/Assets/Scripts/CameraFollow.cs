using UnityEngine;

public class Follow_player : MonoBehaviour
{
    [Header("Default Target")]
    public Transform player;   // Player is the normal follow target

    [Header("Fixed Camera Settings")]
    public float fixedY = 4.25f;
    public float fixedZ = -10f;

    private Transform overrideTarget;  // Used when a trigger tells the camera to focus elsewhere

    void LateUpdate()
    {
        if (!player) return;

        // Use override target if one exists, otherwise follow the player
        Transform target = overrideTarget != null ? overrideTarget : player;

        transform.position = new Vector3(
            target.position.x,
            fixedY,
            fixedZ
        );
    }

    // Methods to control the override
    public void SetOverrideTarget(Transform newTarget)
    {
        overrideTarget = newTarget;
    }

    public void ClearOverrideTarget()
    {
        overrideTarget = null;
    }
}
