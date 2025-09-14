using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Defaults")]
    [SerializeField] float defaultDuration = 0.10f;
    [SerializeField] float defaultMagnitude = 0.15f;
    [SerializeField] float defaultFrequency = 40f;

    Vector3 restLocalPos;
    float timer;
    float duration;
    float magnitude;
    float stepInterval;    // seconds per jitter step
    float nextStepAt;

    void Awake()
    {
        restLocalPos = transform.localPosition;
    }

    void OnEnable()
    {
        transform.localPosition = restLocalPos;
        timer = 0f;
    }

    // Call this from your shooting code
    public void Shake(float dur = -1f, float mag = -1f, float freq = -1f)
    {
        if (dur < 0) dur = defaultDuration;
        if (mag < 0) mag = defaultMagnitude;
        if (freq < 0) freq = defaultFrequency;

        duration = dur;
        magnitude = mag;
        stepInterval = 1f / Mathf.Max(1f, freq);

        timer = duration;
        nextStepAt = 0f; // force immediate offset
    }

    void LateUpdate()
    {
        if (timer > 0f)
        {
            // use unscaled so pausing time doesn't kill the shake
            float dt = Time.unscaledDeltaTime;
            timer -= dt;

            if (Time.unscaledTime >= nextStepAt)
            {
                // new random offset
                Vector2 rnd = Random.insideUnitCircle * magnitude;
                transform.localPosition = restLocalPos + new Vector3(rnd.x, rnd.y, 0f);
                nextStepAt = Time.unscaledTime + stepInterval;
            }

            if (timer <= 0f)
            {
                transform.localPosition = restLocalPos;
            }
        }
    }

    void OnDisable()
    {
        transform.localPosition = restLocalPos;
    }
}
