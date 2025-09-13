using UnityEngine;

public class GunAiming_Generated : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform gunPivot;             // rotates to aim
    [SerializeField] private SpriteRenderer[] spritesToFlip; // player/gun sprites to mirror visually

    [Header("Input hookup")]
    [SerializeField] private InputSystem_Actions controls;

    Camera cam;
    Vector2 screenPos;
    Vector2 stick;
    [SerializeField] float stickDeadzone = 0.15f;

    void Awake()
    {
        cam = Camera.main;
        if (controls == null) controls = new InputSystem_Actions();
        controls.Gameplay.AimPoint.performed += c => screenPos = c.ReadValue<Vector2>();
    }
    void OnEnable() => controls.Gameplay.Enable();
    void OnDisable() => controls.Gameplay.Disable();

    void Update()
    {
        Vector2 aimDir = Vector2.zero;

        if (stick.sqrMagnitude >= stickDeadzone * stickDeadzone)
        {
            aimDir = stick.normalized;
        }
        else
        {
            Vector3 w = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            w.z = 0f;
            Vector2 to = (Vector2)(w - gunPivot.position);
            if (to.sqrMagnitude > 0.0001f) aimDir = to.normalized;
        }

        if (aimDir.sqrMagnitude > 0f)
        {
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            gunPivot.rotation = Quaternion.Euler(0, 0, angle);

            bool facingLeft = aimDir.x < 0f;
            foreach (var sr in spritesToFlip)
                if (sr) sr.flipX = facingLeft;
        }
    }
}
