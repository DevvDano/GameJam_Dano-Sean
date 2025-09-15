using UnityEditor.SearchService;
using UnityEngine;

public class Level1Trigger : MonoBehaviour
{
    [SerializeField] private string sceneSelector;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneSelector);
        }
    }
}
