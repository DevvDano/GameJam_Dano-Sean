using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] int maxHP = 3;

    [Header("Optional FX")]
    [SerializeField] Animator animator;
    [SerializeField] AudioSource hurtSfx;
    [SerializeField] AudioSource deathSfx;
    [SerializeField] GameObject deathVfxPrefab;

    int currentHP;

    void Awake()
    {
        currentHP = maxHP;
        if (!animator) animator = GetComponent<Animator>();
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= Mathf.Max(1, dmg);

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        if (hurtSfx) hurtSfx.Play();
        if (animator) animator.SetTrigger("Hurt");
    }

    void Die()
    {
        if (deathSfx) deathSfx.Play();
        if (animator) animator.SetTrigger("Die");
        if (deathVfxPrefab) Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);

        // Destroy after short delay (let anim/sound play)
        Destroy(gameObject, 0.1f);
    }
}
