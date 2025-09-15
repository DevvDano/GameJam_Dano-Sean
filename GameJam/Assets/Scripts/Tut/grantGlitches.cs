using UnityEngine;

public class GlitchUpgradeTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        PlayerMovement pm = other.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.EnableGlitchUpgrades();
        }

        // Optional: destroy the trigger object after activation
        Destroy(gameObject);
    }
}
