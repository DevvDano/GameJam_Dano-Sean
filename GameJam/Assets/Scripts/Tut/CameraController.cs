using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform currentRoom;

    [Header("Follow Settings")]
    [SerializeField] private bool followY = false;
    [SerializeField] private float ySmooth = 6f;
    [SerializeField] private Vector2 yLimits = new Vector2(-9999f, 9999f);
    [SerializeField] private float yOffset = 0f;   // global/active offset
    [SerializeField] private float fixedZ = -10f;

    private void Awake()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    private void LateUpdate()
    {
        if (!currentRoom) return;

        float targetX = currentRoom.position.x;
        float targetY;

        if (followY && player)
        {
            float desiredY = Mathf.Clamp(player.position.y, yLimits.x, yLimits.y) + yOffset;
            targetY = Mathf.Lerp(transform.position.y, desiredY, ySmooth * Time.deltaTime);
        }
        else
        {
            targetY = currentRoom.position.y + yOffset;
        }

        transform.position = new Vector3(targetX, targetY, fixedZ);
    }

    public void MoveToNewRoom(Transform room)
    {
        currentRoom = room;
        transform.position = new Vector3(room.position.x, transform.position.y, fixedZ);
    }

    // Add near your other fields
private Coroutine yOffsetTween;

// Keep your existing setter
public void SetYOffset(float value) => yOffset = value;

// NEW: Smooth setter
public void SetYOffsetSmooth(float target, float duration)
{
    if (yOffsetTween != null) StopCoroutine(yOffsetTween);
    yOffsetTween = StartCoroutine(LerpYOffset(target, duration));
}

private System.Collections.IEnumerator LerpYOffset(float target, float duration)
{
    float start = yOffset;
    float t = 0f;

    while (t < duration)
    {
        t += Time.deltaTime; // use Time.unscaledDeltaTime if you pause time
        float k = duration > 0f ? t / duration : 1f;
        yOffset = Mathf.Lerp(start, target, k);
        yield return null;
    }

    yOffset = target;
    yOffsetTween = null;
}

    public void LockCurrentY()
    {
    if (currentRoom == null) return;
    // Keep camera exactly where it is vertically, but stop following Y
    yOffset = transform.position.y - currentRoom.position.y;
    followY = false;
    }

    public bool FollowY => followY;
    public void SetFollowY(bool enabled) => followY = enabled;
    public void SetPlayer(Transform t) => player = t;
    public float GetYOffset() => yOffset; 
}
