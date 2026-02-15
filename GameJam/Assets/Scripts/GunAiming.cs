using UnityEngine;
using UnityEngine.InputSystem;

public class GunAiming : MonoBehaviour
{
    [Header("Transforms")]
    [Tooltip("Empty object at the wrist/hand. THIS rotates. Do NOT use FirePoint here.")]
    [SerializeField] private Transform gunPivot;

    [Tooltip("The gun's SpriteRenderer (for vertical flip when aiming left).")]
    [SerializeField] private SpriteRenderer gunSprite;

    [Tooltip("Optional: other sprites to face the aim direction (flipX). Do NOT include gunSprite here.")]
    [SerializeField] private SpriteRenderer[] spritesToFlip;

    [Header("Input (Input System)")]
    [Tooltip("Screen position (Vector2). Bind to <Pointer>/position or your AimPoint action.")]
    [SerializeField] private InputActionReference aimPointAction;

    [Tooltip("Stick aim (Vector2). Bind to <Gamepad>/rightStick (and/or mouse delta if you want).")]
    [SerializeField] private InputActionReference aimStickAction;

    [Header("Options")]
    [Tooltip("Flip the gun vertically when aiming left so sprite isn't upside down.")]
    [SerializeField] private bool flipGunVerticallyWhenLeft = true;

    [Tooltip("Also flip these sprites on X so the body faces the aim.")]
    [SerializeField] private bool makeBodyFaceAim = false;

    [Tooltip("Minimum stick magnitude before we consider stick aim.")]
    [SerializeField] private float stickDeadzone = 0.15f;

    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
        if (!gunPivot) gunPivot = transform; // safe fallback
    }

    void OnEnable()
    {
        if (aimPointAction) aimPointAction.action.Enable();
        if (aimStickAction) aimStickAction.action.Enable();
    }

    void OnDisable()
    {
        if (aimPointAction) aimPointAction.action.Disable();
        if (aimStickAction) aimStickAction.action.Disable();
    }

    void LateUpdate()
    {
        Vector2 aimDir = GetAimDirection();
        if (aimDir.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        gunPivot.rotation = Quaternion.Euler(0f, 0f, angle);

        bool aimingLeft = (angle > 90f || angle < -90f);

        // Flip gun vertically when aiming left (keeps scale intact)
        if (gunSprite && flipGunVerticallyWhenLeft)
            gunSprite.flipY = aimingLeft;

        // Optionally flip body sprites to face aim direction
        if (makeBodyFaceAim && spritesToFlip != null)
        {
            for (int i = 0; i < spritesToFlip.Length; i++)
            {
                var sr = spritesToFlip[i];
                if (!sr || sr == gunSprite) continue;
                sr.flipX = aimingLeft;
            }
        }
    }

    private Vector2 GetAimDirection()
    {
        // 1) Right stick (if provided)
        if (aimStickAction && aimStickAction.action.enabled)
        {
            Vector2 stick = aimStickAction.action.ReadValue<Vector2>();
            if (stick.sqrMagnitude >= stickDeadzone * stickDeadzone)
                return stick.normalized;
        }

        // 2) Screen position action (mouse/touch)
        if (cam)
        {
            Vector2 screenPos = Vector2.zero;
            if (aimPointAction && aimPointAction.action.enabled)
                screenPos = aimPointAction.action.ReadValue<Vector2>();
            else if (Mouse.current != null)
                screenPos = Mouse.current.position.ReadValue();

            if (screenPos != Vector2.zero)
            {
                Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
                world.z = 0f;
                Vector2 dir = (Vector2)(world - gunPivot.position);
                if (dir.sqrMagnitude > 0.0001f) return dir.normalized;
            }
        }

        return Vector2.zero;
    }

    // Optional: let other scripts feed aim directly (e.g., from a virtual joystick)
    public void AimVector(Vector2 aim)
    {
        if (aim.sqrMagnitude < 0.0001f) return;
        float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
        gunPivot.rotation = Quaternion.Euler(0f, 0f, angle);

        bool aimingLeft = (angle > 90f || angle < -90f);
        if (gunSprite && flipGunVerticallyWhenLeft)
            gunSprite.flipY = aimingLeft;

        if (makeBodyFaceAim && spritesToFlip != null)
        {
            foreach (var sr in spritesToFlip)
            {
                if (!sr || sr == gunSprite) continue;
                sr.flipX = aimingLeft;
            }
        }
    }
}
