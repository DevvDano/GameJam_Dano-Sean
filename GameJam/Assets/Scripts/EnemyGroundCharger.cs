using System.Collections;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class EnemyGroundCharger : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] float patrolSpeed = 2f;
    [SerializeField] Transform leftBound;
    [SerializeField] Transform rightBound;


    [Header("Aggro & Charge")]
    [SerializeField] float aggroRange = 6f;
    [SerializeField] float windupTime = 0.35f;
    [SerializeField] float chargeSpeed = 12f;
    [SerializeField] float chargeDuration = 0.35f;
    [SerializeField] float cooldown = 1.0f;


    Rigidbody2D rb;
    Transform player;
    bool movingRight = true;
    bool charging;
    bool cooling;
    float dir = 1f;


    void Awake() => rb = GetComponent<Rigidbody2D>();


    void Update()
    {
        if (!player) player = Targeting.FindPlayer();
        if (charging || cooling) return;


        if (player && Vector2.Distance(transform.position, player.position) <= aggroRange)
        {
            StartCoroutine(ChargeRoutine());
            return;
        }


        Patrol();
    }


    void Patrol()
    {
        dir = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * patrolSpeed, rb.linearVelocity.y);


        if (movingRight && transform.position.x >= rightBound.position.x) movingRight = false;
        else if (!movingRight && transform.position.x <= leftBound.position.x) movingRight = true;


        transform.localScale = new Vector3(dir, 1, 1);
    }


    IEnumerator ChargeRoutine()
    {
        charging = true;
        if (player) dir = Mathf.Sign(player.position.x - transform.position.x);
        transform.localScale = new Vector3(dir, 1, 1);


        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // wind-up stop
        yield return new WaitForSeconds(windupTime);


        float t = 0f;
        while (t < chargeDuration)
        {
            rb.linearVelocity = new Vector2(dir * chargeSpeed, rb.linearVelocity.y);
            t += Time.deltaTime;
            yield return null;
        }


        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        charging = false;
        cooling = true;
        yield return new WaitForSeconds(cooldown);
        cooling = false;
    }


#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        if (leftBound && rightBound)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(leftBound.position, rightBound.position);
        }
    }
#endif
}