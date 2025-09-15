using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] float defaultDuration = 0.10f;
    [SerializeField] float defaultMagnitude = 0.15f;
    [SerializeField] float defaultFrequency = 40f;
    [SerializeField] AnimationCurve envelope = AnimationCurve.EaseInOut(0,1,1,0); // smooth fade

    Vector3 restLocalPos;
    float timer, duration, mag, freq;
    float seedX, seedY;

    void Awake()
    {
        restLocalPos = transform.localPosition;
        seedX = Random.value * 1000f;
        seedY = Random.value * 1000f + 100f;
    }

    public void Shake(float duration = -1f, float mag = -1f, float freq = -1f)
    {
        this.duration = duration < 0 ? defaultDuration   : duration;
        this.mag      = mag      < 0 ? defaultMagnitude  : mag;
        this.freq     = freq     < 0 ? defaultFrequency  : freq;

        timer = this.duration;
    }

    void LateUpdate()
    {
        if (timer <= 0f) return;

        timer -= Time.unscaledDeltaTime;

        // 0..1 time into the shake
        float t01 = 1f - Mathf.Clamp01(timer / Mathf.Max(0.0001f, duration));
        float amp = mag * envelope.Evaluate(t01); // fade with curve

        // Continuous noise in [-1,1]
        float t = Time.unscaledTime * freq * 0.1f; // reduce scaling to calm motion
        float nx = Mathf.PerlinNoise(seedX, t) * 2f - 1f;
        float ny = Mathf.PerlinNoise(seedY, t) * 2f - 1f;

        Vector3 offset = new Vector3(nx, ny, 0f) * amp;
        transform.localPosition = restLocalPos + offset;

        if (timer <= 0f) transform.localPosition = restLocalPos;
    }

    void OnDisable() => transform.localPosition = restLocalPos;
}
