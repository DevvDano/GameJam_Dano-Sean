using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    [SerializeField] private string sceneName;

    //On Player2d collision with this object, load sceneName
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //Load the next scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}
