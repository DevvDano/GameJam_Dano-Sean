using UnityEngine;

public class downwardTriggerPole : MonoBehaviour
{
    [SerializeField] private upwardTriggerPole targetPole; // assign in Inspector

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (targetPole != null)
        {
            targetPole.downwardTrigger();
        }
    }
}
