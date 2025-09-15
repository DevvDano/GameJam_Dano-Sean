using UnityEngine;

public class FadeTrigger : MonoBehaviour
{
    [SerializeField] private ScreenFader fader;
    [SerializeField] private bool fadeToBlack = true;
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (fadeToBlack) fader.FadeToBlack();
        else fader.FadeFromBlack();
    }
}
