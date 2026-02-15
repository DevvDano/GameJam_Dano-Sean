using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] int maxHP = 5;
    [SerializeField] float invulnTime = 0.75f;

    [Header("FX (optional)")]
    [SerializeField] Animator animator;
    [SerializeField] AudioSource hurtSfx;
    [SerializeField] AudioSource deathSfx;

    [Header("Visual Flash")]
    [SerializeField] SpriteRenderer spriteRenderer;  // assign in Inspector
    [SerializeField] Color flashColor = Color.red;
    [SerializeField] float flashInterval = 0.1f;     // how fast to blink

    public int hp;
    bool invulnerable;
    Color originalColor;

    void Awake()
    {
        hp = maxHP;
        if (!animator) animator = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer) originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int dmg)
    {
        if (invulnerable) return;
        hp -= Mathf.Max(1, dmg);

        if (hp <= 0)
        {
            if (deathSfx) deathSfx.Play();
            if (animator) animator.SetTrigger("Die");
            StartCoroutine(DieAndSignal());
            return;
        }

        if (hurtSfx) hurtSfx.Play();
        if (animator) animator.SetTrigger("Hurt");
        StartCoroutine(Invulnerability());
    }

    IEnumerator Invulnerability()
    {
        invulnerable = true;

        // flash red while invulnerable
        float elapsed = 0f;
        while (elapsed < invulnTime)
        {
            if (spriteRenderer)
            {
                spriteRenderer.color = flashColor;
                yield return new WaitForSeconds(flashInterval);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(flashInterval);
            }
            else
            {
                yield return null;
            }

            elapsed += flashInterval * 2f;
        }

        if (spriteRenderer) spriteRenderer.color = originalColor;
        invulnerable = false;
    }

    IEnumerator DieAndSignal()
    {
        var ctrl = GetComponent<MonoBehaviour>();
        if (ctrl) ctrl.enabled = false;

        yield return new WaitForSeconds(0.5f);
        GameEvents.PlayerDied?.Invoke();
        Destroy(gameObject);
    }

    // Current Health Methods
    public void AddHealth(int amount)
    {
        maxHP += Mathf.Max(1, amount);
        hp += Mathf.Max(1, amount);
    }

    public void Heal(int amount) => hp = Mathf.Min(maxHP, hp + Mathf.Max(1, amount));

    public int CurrentHP => hp;
    public int MaxHP => maxHP;
}
