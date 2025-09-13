// GunAiming_Generated.cs
using UnityEngine;

public class GunAiming_Generated : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform gunPivot;   // the transform that rotates (weapon root)
    [SerializeField] private Transform playerRoot; // optional: flip sprite on X

    [Header("Tuning")]
    [SerializeField] private float stickDeadzone = 0.15f;

    InputSystem_Actions controls;
    Camera cam;
    Vector2 lastScreenPoint; // from AimPoint
    Vector2 lastStick;       // from AimStick

    void Awake()
    {
        cam = Camera.main;
        controls = new InputSystem_Actions();

        // Cache latest values from actions
        controls.Gameplay.AimPoint.performed += ctx => lastScreenPoint = ctx.ReadValue<Vector2>();
    }

    void OnEnable() => controls.Gameplay.Enable();
    void OnDisable() => controls.Gameplay.Disable();

    void Update()
    {
        Vector2 aimDir = Vector2.zero;

        // Prefer right-stick if meaningful input
        if (lastStick.sqrMagnitude >= stickDeadzone * stickDeadzone)
        {
            aimDir = lastStick.normalized;
        }
        else
        {
            // Mouse/touch: convert screen point to world, then vector from gun to that point
            if (cam)
            {
                var world = cam.ScreenToWorldPoint(new Vector3(lastScreenPoint.x, lastScreenPoint.y, Mathf.Abs(cam.transform.position.z)));
                Vector2 toCursor = (Vector2)(world - gunPivot.position);
                if (toCursor.sqrMagnitude > 0.0001f) aimDir = toCursor.normalized;
            }
        }

        if (aimDir.sqrMagnitude > 0f)
        {
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            gunPivot.rotation = Quaternion.Euler(0, 0, angle);

            if (playerRoot) // optional visual flip
            {
                var s = playerRoot.localScale;
                s.x = Mathf.Abs(s.x) * (aimDir.x >= 0 ? 1f : -1f);
                playerRoot.localScale = s;
            }
        }
    }
}
