using System.Collections;
using UnityEngine;

public class CameraCutsceneTrigger2D : MonoBehaviour
{
    [Header("Focus Target (where to pan)")]
    public Transform focusTarget;

    [Header("Pan Settings")]
    [Tooltip("How long the camera should take to reach the target Y.")]
    public float panDuration = 1.0f;
    [Tooltip("Extra vertical offset added to focus target Y.")]
    public float yOffset = 0f;
    [Tooltip("Pan horizontally too? If false, X stays where it is during the cutscene.")]
    public bool panX = false;

    [Header("Zoom (Orthographic)")]
    public bool changeZoom = true;
    public float targetOrthoSize = 7f;     // zoom OUT if larger than current
    public float zoomDuration = 0.6f;      // how fast to zoom
    public float zoomDelay = 0f;           // wait this many seconds before zooming

    [Header("After Cutscene")]
    [Tooltip("Lock Follow_player's fixedY to wherever the camera ended.")]
    public bool lockNewYAfter = true;
    [Tooltip("Also set override target so the camera keeps following this on X after?")]
    public bool setOverrideTarget = true;

    [Header("Misc")]
    [Tooltip("Prevent re-triggering this cutscene more than once.")]
    public bool oneShot = true;

    bool hasFired;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (oneShot && hasFired) return;
        if (!Camera.main || !focusTarget) return;

        hasFired = true;
        StartCoroutine(RunCutscene(Camera.main));
    }

    IEnumerator RunCutscene(Camera cam)
    {
        var follow = cam.GetComponent<Follow_player>();
        if (!follow) yield break;

        // Disable the script that locks Y (and sets X) each frame
        follow.enabled = false;

        // Cache start and end positions
        Vector3 startPos = cam.transform.position;

        float endX = panX ? focusTarget.position.x : startPos.x;
        float endY = focusTarget.position.y + yOffset;
        Vector3 endPos = new Vector3(endX, endY, startPos.z);

        // Optionally start a zoom coroutine in parallel
        float originalSize = cam.orthographicSize;
        Coroutine zoomCo = null;
        if (changeZoom && cam.orthographic)
        {
            zoomCo = StartCoroutine(ZoomRoutine(cam, originalSize, targetOrthoSize, zoomDuration, zoomDelay));
        }

        // Smooth pan (ease-in-out)
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, panDuration);
            float a = EaseInOut(t);
            cam.transform.position = Vector3.Lerp(startPos, endPos, a);
            yield return null;
        }

        // Ensure exact end
        cam.transform.position = endPos;

        // Wait for zoom to finish if it’s still running
        if (zoomCo != null) yield return zoomCo;

        // Re-enable follow and lock Y to the new height if desired
        if (lockNewYAfter)
        {
            follow.fixedY = cam.transform.position.y;
        }

        if (setOverrideTarget)
        {
            follow.SetOverrideTarget(focusTarget); // keeps following focus on X (your script only follows X)
        }

        follow.enabled = true;

        // Optional: if you NEVER want this to run again, disable the collider
        if (oneShot)
        {
            var col = GetComponent<Collider2D>();
            if (col) col.enabled = false;
        }
    }

    IEnumerator ZoomRoutine(Camera cam, float from, float to, float duration, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            float a = EaseInOut(t);
            cam.orthographicSize = Mathf.Lerp(from, to, a);
            yield return null;
        }
        cam.orthographicSize = to;
    }

    // Smooth step-like easing (0->1 with ease in/out)
    float EaseInOut(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * (3f - 2f * x);
    }
}