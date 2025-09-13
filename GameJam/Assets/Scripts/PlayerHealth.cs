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

    int hp;
    bool invulnerable;

    void Awake()
    {
        hp = maxHP;
        if (!animator) animator = GetComponent<Animator>();
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
        yield return new WaitForSeconds(invulnTime);
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


    public void Heal(int amount) => hp = Mathf.Min(maxHP, hp + Mathf.Max(1, amount));
    public int CurrentHP => hp;
    public int MaxHP => maxHP;
}
