// PlayerShooting_Generated.cs
using UnityEngine;

public class PlayerShooting_Generated : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform firePoint;             // must point +X along barrel
    [SerializeField] private Projectile projectilePrefab;

    [Header("Stats")]
    [SerializeField] private float fireRate = 6f;             // bullets per second
    [SerializeField] private float projectileDamage = 10f;

    [Header("FX")]
    [SerializeField] private CameraShake camShake;          // drag CameraRig here
    [SerializeField] private ParticleSystem muzzleFlash;      // under firePoint, Loop OFF, RateOverTime 0
    [SerializeField] private AudioSource gunshotSfx;          // on Player, Loop OFF
    [SerializeField] private AudioClip gunshotClip;           // assign clip

    private InputSystem_Actions controls; // generated input
    private bool isFiring;
    private float cooldown;

    void Awake()
    {
        controls = new InputSystem_Actions();
        controls.Gameplay.Fire.started += _ => isFiring = true;
        controls.Gameplay.Fire.canceled += _ =>
        {
            isFiring = false;
            if (muzzleFlash)
                muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        };

        if (camShake == null)
            camShake = FindAnyObjectByType<CameraShake>();
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

        // 1) Spawn projectile
        var proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        proj.gameObject.layer = LayerMask.NameToLayer("PlayerProjectile");
        proj.transform.localScale = projectilePrefab.transform.localScale;

        // 2) Initialize direction/damage
        Vector2 dir = (Vector2)firePoint.right; // make sure firePoint +X is forward
        proj.Initialize(dir.normalized, projectileDamage);

        // 3) Ignore player collisions
        var projCol = proj.GetComponent<Collider2D>();
        if (projCol != null)
        {
            var playerColliders = GetComponentsInParent<Collider2D>();
            foreach (var pc in playerColliders)
                if (pc && pc.enabled) Physics2D.IgnoreCollision(projCol, pc, true);
        }

        // 4) Camera shake
        camShake?.Shake(0.12f, 0.18f, 40f);

        // 5) Muzzle flash (one-shot burst, no looping)
        if (muzzleFlash)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.Emit(Random.Range(8, 14));
        }

        // 6) Gunshot audio
        if (gunshotSfx && gunshotClip)
            gunshotSfx.PlayOneShot(gunshotClip);
    }
}
