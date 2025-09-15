using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Transform previousRoom;
    [SerializeField] private Transform nextRoom;
    [SerializeField] private CameraController cam;

    [Header("Camera Behavior")]
    [SerializeField] private bool followYWhenGoingToNext = true;
    [SerializeField] private bool followYWhenGoingToPrevious = false;

    [Header("Per-Direction Y Offsets")]
    [SerializeField] private float yOffsetWhenGoingToNext = 0f;
    [SerializeField] private float yOffsetWhenGoingToPrevious = 0f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || !cam) return;

        bool goingToNext = collision.transform.position.x < transform.position.x;

        // Apply per-direction follow-Y and per-direction offset
        cam.SetFollowY(goingToNext ? followYWhenGoingToNext : followYWhenGoingToPrevious);
        cam.SetYOffset(goingToNext ? yOffsetWhenGoingToNext : yOffsetWhenGoingToPrevious);

        if (goingToNext)
            cam.MoveToNewRoom(nextRoom);
        else
            cam.MoveToNewRoom(previousRoom);
    }
}
