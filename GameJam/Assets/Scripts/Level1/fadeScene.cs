using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadePanel; 
    [SerializeField] private float fadeDuration = 1f;

    public void FadeToBlack()
    {
        StartCoroutine(Fade(1f)); // alpha 1 = black
    }

    public void FadeFromBlack()
    {
        StartCoroutine(Fade(0f)); // alpha 0 = clear
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadePanel.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            yield return null;
        }

        fadePanel.alpha = targetAlpha;
    }
}
