using System.Collections;
using UnityEngine;


public class EnemyTurret : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] float range = 10f;


    [Header("Shooting")]
    [SerializeField] GameObject projectilePrefab; // EnemyProjectile
    [SerializeField] Transform firePoint;
    [SerializeField] float projectileSpeed = 12f;
    [SerializeField] int burstCount = 3;
    [SerializeField] float shotInterval = 0.15f;
    [SerializeField] float burstCooldown = 1.5f;


    Transform player;
    bool firing;


    void Update()
    {
        if (!player) player = Targeting.FindPlayer();
        if (!player) return;


        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= range && !firing) StartCoroutine(Burst());


        // face player
        transform.localScale = new Vector3(player.position.x > transform.position.x ? 1f : -1f, 1f, 1f);
    }


    IEnumerator Burst()
    {
        firing = true;
        for (int i = 0; i < burstCount; i++)
        {
            if (projectilePrefab && firePoint && player)
            {
                var p = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
                var rbp = p.GetComponent<Rigidbody2D>();
                if (rbp)
                {
                    var dir = Targeting.DirTo(firePoint, player);
                    rbp.linearVelocity = dir * projectileSpeed;
                }
            }
            yield return new WaitForSeconds(shotInterval);
        }
        yield return new WaitForSeconds(burstCooldown);
        firing = false;
    }


#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, range);
    }
#endif
}