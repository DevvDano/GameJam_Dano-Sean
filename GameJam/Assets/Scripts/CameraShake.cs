// CameraShake2D.cs
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] float defaultDuration = 0.10f;
    [SerializeField] float defaultMagnitude = 0.15f;
    [SerializeField] float defaultFrequency = 40f;

    Vector3 restLocalPos;
    float timer, stepInterval, nextStepAt;
    float magnitude;

    void Awake() => restLocalPos = transform.localPosition;

    public void Shake(float duration = -1f, float mag = -1f, float freq = -1f)
    {
        if (duration < 0) duration = defaultDuration;
        if (mag < 0) mag = defaultMagnitude;
        if (freq < 0) freq = defaultFrequency;

        magnitude = mag;
        stepInterval = 1f / Mathf.Max(1f, freq);
        timer = duration;
        nextStepAt = 0f;
    }

    void LateUpdate()
    {
        if (timer <= 0f) return;

        float dt = Time.unscaledDeltaTime;
        timer -= dt;

        if (Time.unscaledTime >= nextStepAt)
        {
            Vector2 rnd = Random.insideUnitCircle * magnitude;
            transform.localPosition = restLocalPos + new Vector3(rnd.x, rnd.y, 0f);
            nextStepAt = Time.unscaledTime + stepInterval;
        }

        if (timer <= 0f)
            transform.localPosition = restLocalPos;
    }

    void OnDisable() => transform.localPosition = restLocalPos;
}
