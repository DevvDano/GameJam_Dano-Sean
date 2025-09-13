using UnityEngine;

public class PlayerShooting_Generated : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform firePoint;             // where bullets spawn
    [SerializeField] private Projectile projectilePrefab;     // assign your bullet prefab

    [Header("Stats")]
    [SerializeField] private float fireRate = 6f;             // bullets per second
    [SerializeField] private float projectileDamage = 10f;    // damage per bullet

    private InputSystem_Actions controls; // generated input class
    private bool isFiring;
    private float cooldown;

    void Awake()
    {
        controls = new InputSystem_Actions();

        // Fire pressed → start firing
        controls.Gameplay.Fire.started += _ => isFiring = true;
        // Fire released → stop firing
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

        // Spawn projectile
        var proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        proj.gameObject.layer = LayerMask.NameToLayer("PlayerProjectile");

        // Ensure firePoint points +X (right) along barrel
        Vector2 dir = firePoint.right;
        proj.Initialize(dir, projectileDamage);
    }
}
