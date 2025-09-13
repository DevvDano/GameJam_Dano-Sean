// PlayerShooting_Generated.cs
using UnityEngine;

public class PlayerShooting_Generated : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform firePoint;             // muzzle (must point +X)
    [SerializeField] private Projectile projectilePrefab;     // bullet prefab (has Collider2D + Rigidbody2D)

    [Header("Stats")]
    [SerializeField] private float fireRate = 6f;             // bullets per second
    [SerializeField] private float projectileDamage = 10f;    // damage per bullet

    private InputSystem_Actions controls; // generated Input System class
    private bool isFiring;
    private float cooldown;

    void Awake()
    {
        controls = new InputSystem_Actions();
        controls.Gameplay.Fire.started += _ => isFiring = true;
        controls.Gameplay.Fire.canceled += _ => isFiring = false;
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

        // Spawn projectile (unparented so it doesn't inherit Player scaling/flip)
        var proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        proj.gameObject.layer = LayerMask.NameToLayer("PlayerProjectile");

        // Force projectile scale to prefab's native scale (avoids huge/negative scales)
        proj.transform.localScale = projectilePrefab.transform.localScale;

        // Direction = +X of the barrel
        Vector2 dir = (Vector2)firePoint.right;
        proj.Initialize(dir.normalized, projectileDamage);

        // --- Ignore collisions with the Player (so large player collider won't kill the bullet) ---
        var projCol = proj.GetComponent<Collider2D>();
        if (projCol != null)
        {
            // Get all 2D colliders on the player (root and children)
            var playerColliders = GetComponentsInParent<Collider2D>();
            foreach (var pc in playerColliders)
            {
                if (pc != null && pc.enabled)
                    Physics2D.IgnoreCollision(projCol, pc, true);
            }
        }
    }
}
