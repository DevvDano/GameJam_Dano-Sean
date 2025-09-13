using System.Collections;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [SerializeField] int maxHP = 30;

    [SerializeField]
    [Tooltip("Runtime debug only")]
    private int currentHP;

    [Header("FX (optional)")]
    [SerializeField] Animator animator;
    [SerializeField] AudioSource hurtSfx;
    [SerializeField] AudioSource deathSfx;
    [SerializeField] GameObject deathVfxPrefab;
    [SerializeField] float endDelay = 1.0f;

    bool dying;

    void Awake()
    {
        currentHP = maxHP;                
        if (!animator) animator = GetComponent<Animator>();
    }

    public void TakeDamage(int dmg)
    {
        if (dying) return;

        currentHP -= Mathf.Max(1, dmg);    

        if (currentHP <= 0)
        {
            dying = true;
            if (deathSfx) deathSfx.Play();
            if (animator) animator.SetTrigger("Die");
            if (deathVfxPrefab) Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);

            var ai = GetComponent<MonoBehaviour>(); // BossAI 
            if (ai) ai.enabled = false;

            StartCoroutine(EndAfterDelay());
            return;
        }

        if (hurtSfx) hurtSfx.Play();
        if (animator) animator.SetTrigger("Hurt");
    }

    IEnumerator EndAfterDelay()
    {
        yield return new WaitForSeconds(endDelay);
        GameEvents.BossDefeated?.Invoke();
        Destroy(gameObject);
    }

  
    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
}
