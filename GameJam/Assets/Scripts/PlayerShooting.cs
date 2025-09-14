// PlayerShooting_Generated.cs
using UnityEngine;

public class PlayerShooting_Generated : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Projectile projectilePrefab;

    // 👇 Add this
    [Header("Camera")]
    [SerializeField] private CameraShake camShake;

    [Header("Stats")]
    [SerializeField] private float fireRate = 6f;
    [SerializeField] private float projectileDamage = 10f;

    private InputSystem_Actions controls;
    private bool isFiring;
    private float cooldown;

    void Awake()
    {
        controls = new InputSystem_Actions();
        controls.Gameplay.Fire.started += _ => isFiring = true;
        controls.Gameplay.Fire.canceled += _ => isFiring = false;

        // 👇 Safety net in case you forget to assign in Inspector
        if (camShake == null) camShake = FindAnyObjectByType<CameraShake>();
    }

    void OnEnable() => controls.Gameplay.Enable();
    void OnDisable() => controls.Gameplay.Disable();

    void Update()
    {
        cooldown -= Time.deltaTime;
        if (isFiring && cooldown <= 0f)
        {
            Shoot();
            cooldown = 1f / Mathf.Max(0.01f, fireRate);
        }
    }

    void Shoot()
    {
        if (!projectilePrefab || !firePoint) return;

        // Spawn and init
        var proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        proj.gameObject.layer = LayerMask.NameToLayer("PlayerProjectile");
        proj.transform.localScale = projectilePrefab.transform.localScale;
        Vector2 dir = (Vector2)firePoint.right;        // Make sure your muzzle points +X
        proj.Initialize(dir.normalized, projectileDamage);

        // Ignore player collisions (your existing code)
        var projCol = proj.GetComponent<Collider2D>();
        if (projCol != null)
        {
            var playerColliders = GetComponentsInParent<Collider2D>();
            foreach (var pc in playerColliders)
                if (pc != null && pc.enabled) Physics2D.IgnoreCollision(projCol, pc, true);
        }

        // 👇 Shake right as the projectile leaves the gun
        if (camShake != null) camShake.Shake(0.12f, 0.18f, 40f);
    }
}
